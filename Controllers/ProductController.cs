using StoreManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace StoreManagementSystem.Controllers
{
    public class ProductController : Controller
    {
        private DataAccess da = new DataAccess();

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            DataTable dt = da.ExecuteQuery("SELECT * FROM Product");
            return View(dt);
        }

        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(Product model)
        {
            if (ModelState.IsValid)
            {
                string checkQuery = "SELECT COUNT(*) FROM Product WHERE ProductID = @ProductID";
                SqlParameter[] checkParams = { new SqlParameter("@ProductID", model.ProductID.Trim().ToUpper()) };
                DataTable dt = da.ExecuteQuery(checkQuery, checkParams);

                if (Convert.ToInt32(dt.Rows[0][0]) > 0)
                {
                    ViewBag.Message = "Mã sản phẩm đã tồn tại!";
                    return View(model);
                }

                string insertQuery = "INSERT INTO Product (ProductID, ProductName, Price) VALUES (@ProductID, @ProductName, @Price)";
                SqlParameter[] insertParams = {
                    new SqlParameter("@ProductID", model.ProductID.Trim().ToUpper()),
                    new SqlParameter("@ProductName", model.ProductName),
                    new SqlParameter("@Price", model.Price)
                };
                da.ExecuteNonQuery(insertQuery, insertParams);
                return RedirectToAction("List");
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            string query = "SELECT * FROM Product WHERE ProductID = @ProductID";
            SqlParameter[] parameters = { new SqlParameter("@ProductID", id) };
            DataTable dt = da.ExecuteQuery(query, parameters);
            if (dt.Rows.Count > 0)
            {
                Product product = new Product
                {
                    ProductID = dt.Rows[0]["ProductID"].ToString(),
                    ProductName = dt.Rows[0]["ProductName"].ToString(),
                    Price = Convert.ToDecimal(dt.Rows[0]["Price"])
                };
                return View(product);
            }
            return RedirectToAction("List");
        }

        [HttpPost]
        public ActionResult Edit(Product model)
        {
            if (ModelState.IsValid)
            {
                string updateQuery = "UPDATE Product SET ProductName = @ProductName, Price = @Price WHERE ProductID = @ProductID";
                SqlParameter[] updateParams = {
                    new SqlParameter("@ProductName", model.ProductName),
                    new SqlParameter("@Price", model.Price),
                    new SqlParameter("@ProductID", model.ProductID)
                };
                da.ExecuteNonQuery(updateQuery, updateParams);
                return RedirectToAction("List");
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            // Kiểm tra xem sản phẩm có được sử dụng trong hóa đơn không
            string checkQuery = "SELECT COUNT(*) FROM InvoiceDetails WHERE ProductID = @ProductID";
            SqlParameter[] checkParams = { new SqlParameter("@ProductID", id) }; // Tạo mới mảng tham số
            DataTable dt = da.ExecuteQuery(checkQuery, checkParams);

            if (Convert.ToInt32(dt.Rows[0][0]) > 0)
            {
                ViewBag.Message = "Sản phẩm đã được sử dụng trong hóa đơn, không thể xóa!";
                return RedirectToAction("List");
            }

            // Thực hiện xóa
            string deleteQuery = "DELETE FROM Product WHERE ProductID = @ProductID";
            SqlParameter[] deleteParams = { new SqlParameter("@ProductID", id) }; // Tạo mới mảng tham số
            da.ExecuteNonQuery(deleteQuery, deleteParams);
            return RedirectToAction("List");
        }
    }
}