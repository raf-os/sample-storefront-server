using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SampleStorefront.Context;
using SampleStorefront.Models;

namespace SampleStorefront.Services;

public class CategoryService
{
    private readonly IMemoryCache _cache;
    private readonly AppDbContext _db;

    public CategoryService(IMemoryCache cache, AppDbContext db)
    {
        _cache = cache;
        _db = db;
    }

    public class TreeNode
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public List<TreeNode> Children { get; set; } = [];
        public List<string> Keywords { get; set; } = [];
        public List<int> Relationships { get; set; } = [];
        private TreeNode? ParentNode;

        public void AttachNode(TreeNode newChild)
        {
            newChild.ParentNode = this;
            AddRelationship(newChild);
            ParentNode?.AddRelationship(newChild); // Recursively travel upwards and link the whole tree together
            Children.Add(newChild);
        }

        private void AddRelationship(TreeNode node)
        {
            if (!Relationships.Contains(node.Id))
                Relationships.Add(node.Id);

            if (!Keywords.Contains(node.Name))
                Keywords.Add(node.Name);

            ParentNode?.AddRelationship(node);
        }
    }

    private TreeNode? FindNodeById(IEnumerable<TreeNode> nodes, int id)
    {
        foreach (var node in nodes)
        {
            if (node.Id == id)
                return node;

            var found = FindNodeById(node.Children, id);
            if (found != null)
                return found;
        }

        return null;
    }

    private async Task<List<TreeNode>?> CompileCategoryTree()
    {
        // Transforms the 1-dimensional data into a tree graph
        var treeRoots = await _db.Categories
            .Where(r => r.ParentId == null)
            .ToListAsync();

        if (treeRoots == null)
            return null;

        var CompiledTree = new List<TreeNode>();

        foreach (var node in treeRoots)
        {
            CompiledTree.Add(new TreeNode { Id = node.Id, Name = node.Name });
        }

        var treeLeafs = await _db.Categories
            .Where(r => r.ParentId != null)
            .ToListAsync();

        foreach (Category cat in treeLeafs)
        {
            if (cat.ParentId == null)
                continue;
            var parentNode = FindNodeById(CompiledTree, (int)cat.ParentId);

            parentNode?.AttachNode(new TreeNode { Id = cat.Id, Name = cat.Name });
        }

        return CompiledTree;
    }

    public void InvalidateCache()
    {
        _cache.Remove("CategoryTree");
    }

    public async Task<List<TreeNode>?> GetCategoryTree()
    {
        var categoryTree = await _cache.GetOrCreateAsync("CategoryTree", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return await CompileCategoryTree();
        });

        return categoryTree;
    }
}