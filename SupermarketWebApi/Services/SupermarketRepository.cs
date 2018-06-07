using System.Collections.Generic;
using System.Linq;
using SupermarketWebApi.DTO;
using SupermarketWebApi.Helpers;
using SupermarketWebApi.Models;

namespace SupermarketWebApi.Services
{
    public class SupermarketRepository : ISupermarketRepository
    {
        private SupermarketContext _context;
        private IPropertyMappingService _propertyMappingService;
        private IProductPropertyMappingService _productPropertyMappingService;
        private IStaffMemberPropertyMappingService _staffMemberPropertyMappingService;
        private IStockPropertyMappingService _stockPropertyMappingService;


        public SupermarketRepository(SupermarketContext context, IPropertyMappingService propertyMappingService,
            IProductPropertyMappingService productPropertyMappingService, IStaffMemberPropertyMappingService staffMemberPropertyMappingService,
            IStockPropertyMappingService stockPropertyMappingService)
        {
            _context = context;
            _propertyMappingService = propertyMappingService;
            _productPropertyMappingService = productPropertyMappingService;
            _staffMemberPropertyMappingService = staffMemberPropertyMappingService;
            _stockPropertyMappingService = stockPropertyMappingService;
        }

        public void AddProduct(Product product)
        {
            _context.Products.Add(product);
        }
        public void AddStaffMember(StaffMember staffMember)
        {
            _context.StaffMembers.Add(staffMember);
        }
        public void AddSupermarket(Supermarket supermarket)
        {
            _context.Supermarkets.Add(supermarket);
        }
        public void AddSupermarketStock(SupermarketStock supermarketStock)
        {
            _context.SupermarketStocks.Add(supermarketStock);
        }

        public void DeleteProduct(Product product)
        {
            _context.Products.Remove(product);
        }
        public void DeleteStaffMember(StaffMember staffMember)
        {
            _context.StaffMembers.Remove(staffMember);
        }
        public void DeleteSupermarket(Supermarket supermarket)
        {
            _context.Supermarkets.Remove(supermarket);
        }
        public void DeleteSupermarketStock(SupermarketStock supermarketStock)
        {
            _context.SupermarketStocks.Remove(supermarketStock);
        }

        public IEnumerable<Product> GetAllProductsFromSupermarket(int supermarketId)
        {
            var allStockWithSupermarketId = _context.SupermarketStocks
                .Where(s => s.SupermarketId == supermarketId)
                .ToList();

            // enumerate through the entire product list once for every product the store stocks
            var products = GetAllProducts();
            List<Product> productsStockedBySupermarket = new List<Product>();
            foreach (SupermarketStock ss in allStockWithSupermarketId)
            {
                foreach (Product p in products)
                {
                    if (ss.ProductId == p.ProductId)
                    {
                        productsStockedBySupermarket.Add(p);
                        break;
                    }
                }
            }

            // then return that array
            return productsStockedBySupermarket;
        }
        public IEnumerable<Supermarket> GetAllSupermarketsStockingProduct(int productId)
        {
            var allStockWithProductId = _context.SupermarketStocks
                .Where(s => s.ProductId == productId)
                .ToList();
            
            var supermarkets = GetAllSupermarkets();
            List<Supermarket> supermarketsWithProduct = new List<Supermarket>();
            foreach (SupermarketStock ss in allStockWithProductId)
            {
                foreach (Supermarket sm in supermarkets)
                {
                    if (ss.SupermarketId == sm.SupermarketId)
                    {
                        supermarketsWithProduct.Add(sm);
                        break;
                    }
                }
            }

            return supermarketsWithProduct;
        }
        public IEnumerable<StaffMember> GettAllStaffMembersFromSuperMarket(int supermarketId)
        {
            return _context.StaffMembers
                .Where(s => s.SupermarketId == supermarketId)
                .ToList();

        }

        public IEnumerable<Supermarket> GetAllSupermarkets()
        {
            return _context.Supermarkets;
        }
        public IEnumerable<Product> GetAllProducts()
        {
            return _context.Products;
        }
        public IEnumerable<SupermarketStock> GetAllStock()
        {
            return _context.SupermarketStocks;
        }
        public IEnumerable<StaffMember> GetAllStaffMembers()
        {
            return _context.StaffMembers;
        }

        public PagedList<Supermarket> GetAllSupermarkets(SupermarketResourceParameters supermarketResourceParameters)
        {
            //var collectionBeforePaging = _context.Supermarkets.OrderBy(s => s.Location).AsQueryable();

            var collectionBeforePaging =
                _context.Supermarkets.ApplySort(supermarketResourceParameters.OrderBy,
                _propertyMappingService.GetPropertyMapping<SupermarketDTO, Supermarket>());

            if (!string.IsNullOrEmpty(supermarketResourceParameters.SearchQuery))
            {
                // trim & ignore casing
                var searchQueryForWhereClause = supermarketResourceParameters.SearchQuery
                    .Trim().ToLowerInvariant();

                collectionBeforePaging = collectionBeforePaging
                    .Where(s => s.Location.ToLowerInvariant().Contains(searchQueryForWhereClause));
            }

            return PagedList<Supermarket>.Create(collectionBeforePaging,
                supermarketResourceParameters.PageNumber,
                supermarketResourceParameters.PageSize);
                
        }
        public PagedList<Product> GetAllProducts(ProductResourceParameters productResourceParameters)
        {
            var collectionBeforePaging =
                _context.Products.ApplySort(productResourceParameters.OrderBy,
                _productPropertyMappingService.GetPropertyMapping<ProductDTO, Product>());

            if (!string.IsNullOrEmpty(productResourceParameters.SearchQuery))
            {
                // trim & ignore casing
                var searchQueryForWhereClause = productResourceParameters.SearchQuery
                    .Trim().ToLowerInvariant();

                collectionBeforePaging = collectionBeforePaging
                    .Where(s => s.Name.ToLowerInvariant().Contains(searchQueryForWhereClause));
            }

            return PagedList<Product>.Create(collectionBeforePaging,
                productResourceParameters.PageNumber,
                productResourceParameters.PageSize);
        }
        public PagedList<StaffMember> GetAllStaffMembers(StaffMemberResourceParameters staffMemberResourceParameters)
        {
            var collectionBeforePaging =
                _context.StaffMembers.ApplySort(staffMemberResourceParameters.OrderBy,
                _staffMemberPropertyMappingService.GetPropertyMapping<StaffMemberDTO, StaffMember>());

            if (!string.IsNullOrEmpty(staffMemberResourceParameters.SearchQuery))
            {
                // trim & ignore casing
                var searchQueryForWhereClause = staffMemberResourceParameters.SearchQuery
                    .Trim().ToLowerInvariant();

                collectionBeforePaging = collectionBeforePaging
                    .Where(s => s.Name.ToLowerInvariant().Contains(searchQueryForWhereClause));
            }

            return PagedList<StaffMember>.Create(collectionBeforePaging,
                staffMemberResourceParameters.PageNumber,
                staffMemberResourceParameters.PageSize);
        }
        public PagedList<SupermarketStock> GetAllStock(StockResourceParameters stockResourceParameters)
        {
            var collectionBeforePaging =
                _context.SupermarketStocks.ApplySort(stockResourceParameters.OrderBy,
                _stockPropertyMappingService.GetPropertyMapping<SupermarketStockDTO, SupermarketStock>());

            return PagedList<SupermarketStock>.Create(collectionBeforePaging,
                stockResourceParameters.PageNumber,
                stockResourceParameters.PageSize);
        }

        public Product GetProductById(int productId)
        {
            return GetAllProducts().FirstOrDefault(p => p.ProductId == productId);
        }
        public StaffMember GetStaffMemberById(int staffMemberId)
        {
            return GetAllStaffMembers().FirstOrDefault(s => s.Id == staffMemberId);
        }
        public Supermarket GetSupermarketById(int supermarketId)
        {
            return GetAllSupermarkets().FirstOrDefault(s => s.SupermarketId == supermarketId);
        }
        public SupermarketStock GetStockById(int id)
        {
            return GetAllStock().FirstOrDefault(s => s.Id == id);
        }

        public SupermarketStock GetStockByProductAndSupermarket(int supermarketId, int productId)
        {
            return _context.SupermarketStocks
                .Where(s => s.SupermarketId == supermarketId && s.ProductId == productId)
                .FirstOrDefault();
        }

        public IEnumerable<SupermarketStock> GetAllStockWithProductId(int productId)
        {
            return _context.SupermarketStocks
                .Where(s => s.ProductId == productId)
                .ToList();
        }
        public IEnumerable<SupermarketStock> GetAllStockWithSupermarketId(int supermarketId)
        {
            return _context.SupermarketStocks
                .Where(s => s.SupermarketId == supermarketId)
                .ToList();
        }
        public IEnumerable<StaffMember> GetAllStaffMembersWithSupermarketId(int supermarketId)
        {
            return _context.StaffMembers
                .Where(s => s.SupermarketId == supermarketId)
                .ToList();
        }

        public bool ProductExists(int productId)
        {
            return _context.Products.Any(p => p.ProductId == productId);
        }
        public bool StaffMemberExists(int staffMemberId)
        {
            return _context.StaffMembers.Any(s => s.Id == staffMemberId);
        }
        public bool SupermarketExists(int supermarketId)
        {
            return _context.Supermarkets.Any(s => s.SupermarketId == supermarketId);
        }
        public bool SupermarketStockExists(int supermarketStockId)
        {
            // checks if a stock exists with the same Id
            return _context.SupermarketStocks.Any(s => s.Id == supermarketStockId);
        }
        public bool SupermarketStockExists(int productId, int supermarketId)
        {
            // checks if a stock exists with the same product and supermarket ids
            return _context.SupermarketStocks.Any(s => s.ProductId == productId && s.SupermarketId == supermarketId);
        }
        public bool SupermarketStockExists(int productId, int supermarketId, int id)
        {
            // checks if a stock exists with the same product and supermarket Ids and is not the row targeted for updating
            return _context.SupermarketStocks.Any(s => s.ProductId == productId && s.SupermarketId == supermarketId && s.Id != id);
        }

        public void UpdateProduct(int productId)
        {
            
        }
        public void UpdateStaffMember(int staffMemberId)
        {
            
        }
        public void UpdateSupermarket(int supermarketId)
        {

        }
        public void UpdateSupermarketStock(int supermarketStockId)
        {
            
        }

        public bool Save()
        {
                return (_context.SaveChanges() >= 0);
        }

        public IEnumerable<Supermarket> GetSupermarketsByIds(IEnumerable<int> supermarketIds)
        {
            return _context.Supermarkets
                .Where(s => supermarketIds.Contains(s.SupermarketId))
                .ToList();
        }
        public IEnumerable<Product> GetProductsByIds(IEnumerable<int> productIds)
        {
            return _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToList();
        }
        public IEnumerable<StaffMember> GetStaffMembersByIds(IEnumerable<int> staffMemberIds)
        {
            return _context.StaffMembers
                .Where(s => staffMemberIds.Contains(s.Id))
                .ToList();
        }
        public IEnumerable<SupermarketStock> GetSupermarketStockByIds(IEnumerable<int> supermarketStockIds)
        {
            return _context.SupermarketStocks
                .Where(s => supermarketStockIds.Contains(s.Id))
                .ToList();
        }

        
    }
}
