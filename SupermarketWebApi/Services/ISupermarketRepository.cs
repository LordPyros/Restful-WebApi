using SupermarketWebApi.Helpers;
using SupermarketWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupermarketWebApi.Services
{
    public interface ISupermarketRepository
    {
        IEnumerable<Supermarket> GetAllSupermarkets();
        IEnumerable<Product> GetAllProducts();
        IEnumerable<StaffMember> GetAllStaffMembers();
        IEnumerable<SupermarketStock> GetAllStock();

        PagedList<Supermarket> GetAllSupermarkets(SupermarketResourceParameters supermarketResourceParameters);
        PagedList<Product> GetAllProducts(ProductResourceParameters productResourceParameters);
        PagedList<StaffMember> GetAllStaffMembers(StaffMemberResourceParameters staffMemberResourceParameters);
        PagedList<SupermarketStock> GetAllStock(StockResourceParameters stockResourceParameters);

        IEnumerable<Supermarket> GetSupermarketsByIds(IEnumerable<int> supermarketIds);
        IEnumerable<Product> GetProductsByIds(IEnumerable<int> productIds);
        IEnumerable<StaffMember> GetStaffMembersByIds(IEnumerable<int> staffMemberIds);
        IEnumerable<SupermarketStock> GetSupermarketStockByIds(IEnumerable<int> supermarketStockIds);

        //IEnumerable<Product> GetAllProductsFromSupermarket(int supermarketId);
        //IEnumerable<StaffMember> GettAllStaffMembersFromSuperMarket(int supermarketId);
        //IEnumerable<Supermarket> GetAllSupermarketsStockingProduct(int productId);

        PagedList<Product> GetAllProductsFromSupermarket(int supermarketId, ProductResourceParameters productResourceParameters);
        PagedList<StaffMember> GetAllStaffMembersFromSupermarket(int supermarketId, StaffMemberResourceParameters staffMemberResourceParameters);

        Supermarket GetSupermarketById(int supermarketId);
        Product GetProductById(int productId);
        StaffMember GetStaffMemberById(int staffMemberId);
        SupermarketStock GetStockById(int Id);
        SupermarketStock GetStockByProductAndSupermarket(int supermarketId, int productId);

        IEnumerable<SupermarketStock> GetAllStockWithProductId(int productId);
        IEnumerable<SupermarketStock> GetAllStockWithSupermarketId(int supermarketId);
        IEnumerable<StaffMember> GetAllStaffMembersWithSupermarketId(int supermarketId);

        void AddSupermarket(Supermarket supermarket);
        void AddProduct(Product product);
        void AddStaffMember(StaffMember staffMember);
        void AddSupermarketStock(SupermarketStock supermarketStock);

        void DeleteSupermarket(Supermarket supermarket);
        void DeleteProduct(Product product);
        void DeleteStaffMember(StaffMember staffMember);
        void DeleteSupermarketStock(SupermarketStock supermarketStock);

        void UpdateProduct(int productId);
        void UpdateStaffMember(int staffMemberId);
        void UpdateSupermarket(int supermarketId);
        void UpdateSupermarketStock(int supermarketStockId);

        bool SupermarketExists(int supermarketId);
        bool ProductExists(int productId);
        bool StaffMemberExists(int staffMemberId);
        bool SupermarketStockExists(int supermarketStockId);
        bool SupermarketStockExists(int productId, int supermarketId);
        bool SupermarketStockExists(int productId, int supermarketId, int id);

        bool Save();
    }
}
