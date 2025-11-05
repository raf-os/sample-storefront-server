using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SampleStorefront.Context;
using SampleStorefront.Models;
using System.Text.Json;

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
        public int? ParentId { get; set; }
        public List<int> ChildIds { get; set; } = [];
        private TreeNode? ParentNode;

        public void AttachNode(TreeNode newChild)
        {
            newChild.ParentId = Id;
            newChild.ParentNode = this;
            AddChildId(newChild.Id); // Add to self
            ParentNode?.AddChildId(newChild.Id); // Recursively add this ID upstream
            Children.Add(newChild);
        }

        private void AddChildId(int childId)
        {
            ChildIds.Add(childId);
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