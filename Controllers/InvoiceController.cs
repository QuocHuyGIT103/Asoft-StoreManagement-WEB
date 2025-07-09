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
    public class InvoiceController : Controller
    {
        private DataAccess da = new DataAccess();

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            DataTable dt = da.ExecuteQuery("SELECT i.InvoiceID, i.CustomerID, c.CustomerName, i.InvoiceDate, i.TotalPrice FROM Invoice i JOIN Customer c ON i.CustomerID = c.CustomerID");
            return View(dt);
        }

        [HttpGet]
        public ActionResult Add()
        {
            ViewBag.Customers = da.ExecuteQuery("SELECT CustomerID, CustomerName FROM Customer");
            ViewBag.Products = da.ExecuteQuery("SELECT ProductID, ProductName, Price FROM Product");
            return View();
        }


        [HttpPost]
        public ActionResult Add(Invoice model, string[] productIds, int[] quantities)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Kiểm tra mảng sản phẩm và số lượng
                    if (productIds == null || quantities == null || productIds.Length == 0 || productIds.Length != quantities.Length)
                    {
                        ViewBag.Message = "Vui lòng chọn ít nhất một sản phẩm và số lượng hợp lệ!";
                        ViewBag.Customers = da.ExecuteQuery("SELECT CustomerID, CustomerName FROM Customer");
                        ViewBag.Products = da.ExecuteQuery("SELECT ProductID, ProductName, Price FROM Product");
                        return View(model);
                    }

                    // Kiểm tra mã hóa đơn đã tồn tại
                    string checkQuery = "SELECT COUNT(*) FROM Invoice WHERE InvoiceID = @InvoiceID";
                    SqlParameter[] checkParams = { new SqlParameter("@InvoiceID", model.InvoiceID.Trim().ToUpper()) };
                    DataTable dt = da.ExecuteQuery(checkQuery, checkParams);

                    if (Convert.ToInt32(dt.Rows[0][0]) > 0)
                    {
                        ViewBag.Message = "Mã hóa đơn đã tồn tại!";
                        ViewBag.Customers = da.ExecuteQuery("SELECT CustomerID, CustomerName FROM Customer");
                        ViewBag.Products = da.ExecuteQuery("SELECT ProductID, ProductName, Price FROM Product");
                        return View(model);
                    }

                    // Tính tổng tiền
                    decimal totalPrice = 0;
                    for (int i = 0; i < productIds.Length; i++)
                    {
                        decimal price = Convert.ToDecimal(da.ExecuteQuery("SELECT Price FROM Product WHERE ProductID = @ProductID", new SqlParameter[] { new SqlParameter("@ProductID", productIds[i]) }).Rows[0]["Price"]);
                        decimal itemTotal = price * quantities[i];
                        totalPrice += itemTotal;
                    }

                    // Lưu hóa đơn
                    string insertInvoiceQuery = "INSERT INTO Invoice (InvoiceID, CustomerID, InvoiceDate, TotalPrice) VALUES (@InvoiceID, @CustomerID, @InvoiceDate, @TotalPrice)";
                    SqlParameter[] invoiceParams = {
                        new SqlParameter("@InvoiceID", model.InvoiceID.Trim().ToUpper()),
                        new SqlParameter("@CustomerID", model.CustomerID),
                        new SqlParameter("@InvoiceDate", DateTime.Now),
                        new SqlParameter("@TotalPrice", totalPrice)
                    };
                    da.ExecuteNonQuery(insertInvoiceQuery, invoiceParams);

                    // Lưu chi tiết hóa đơn
                    for (int i = 0; i < productIds.Length; i++)
                    {
                        decimal price = Convert.ToDecimal(da.ExecuteQuery("SELECT Price FROM Product WHERE ProductID = @ProductID", new SqlParameter[] { new SqlParameter("@ProductID", productIds[i]) }).Rows[0]["Price"]);
                        decimal itemTotal = price * quantities[i];
                        string insertDetailQuery = "INSERT INTO InvoiceDetails (InvoiceID, ProductID, Quantity, TotalPrice) VALUES (@InvoiceID, @ProductID, @Quantity, @TotalPrice)";
                        SqlParameter[] detailParams = {
                            new SqlParameter("@InvoiceID", model.InvoiceID.Trim().ToUpper()),
                            new SqlParameter("@ProductID", productIds[i]),
                            new SqlParameter("@Quantity", quantities[i]),
                            new SqlParameter("@TotalPrice", itemTotal)
                        };
                        da.ExecuteNonQuery(insertDetailQuery, detailParams);
                    }

                    return RedirectToAction("List");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Lỗi khi lưu hóa đơn: " + ex.Message;
                ViewBag.Customers = da.ExecuteQuery("SELECT CustomerID, CustomerName FROM Customer");
                ViewBag.Products = da.ExecuteQuery("SELECT ProductID, ProductName, Price FROM Product");
                return View(model);
            }

            ViewBag.Customers = da.ExecuteQuery("SELECT CustomerID, CustomerName FROM Customer");
            ViewBag.Products = da.ExecuteQuery("SELECT ProductID, ProductName, Price FROM Product");
            return View(model);
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            string query = "SELECT * FROM Invoice WHERE InvoiceID = @InvoiceID";
            SqlParameter[] parameters = { new SqlParameter("@InvoiceID", id) };
            DataTable dt = da.ExecuteQuery(query, parameters);
            if (dt.Rows.Count > 0)
            {
                Invoice invoice = new Invoice
                {
                    InvoiceID = dt.Rows[0]["InvoiceID"].ToString(),
                    CustomerID = dt.Rows[0]["CustomerID"].ToString(),
                    InvoiceDate = Convert.ToDateTime(dt.Rows[0]["InvoiceDate"]),
                    TotalPrice = Convert.ToDecimal(dt.Rows[0]["TotalPrice"])
                };
                ViewBag.Customers = da.ExecuteQuery("SELECT CustomerID, CustomerName FROM Customer");
                return View(invoice);
            }
            return RedirectToAction("List");
        }

        [HttpPost]
        public ActionResult Edit(Invoice model)
        {
            if (ModelState.IsValid)
            {
                string updateQuery = "UPDATE Invoice SET CustomerID = @CustomerID, InvoiceDate = @InvoiceDate, TotalPrice = @TotalPrice WHERE InvoiceID = @InvoiceID";
                SqlParameter[] updateParams = {
                    new SqlParameter("@CustomerID", model.CustomerID),
                    new SqlParameter("@InvoiceDate", model.InvoiceDate),
                    new SqlParameter("@TotalPrice", model.TotalPrice),
                    new SqlParameter("@InvoiceID", model.InvoiceID)
                };
                da.ExecuteNonQuery(updateQuery, updateParams);
                return RedirectToAction("List");
            }
            ViewBag.Customers = da.ExecuteQuery("SELECT CustomerID, CustomerName FROM Customer");
            return View(model);
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            try
            {
                // Xóa chi tiết hóa đơn trước
                string deleteDetailsQuery = "DELETE FROM InvoiceDetails WHERE InvoiceID = @InvoiceID";
                SqlParameter[] detailParams = { new SqlParameter("@InvoiceID", id) };
                da.ExecuteNonQuery(deleteDetailsQuery, detailParams);

                // Xóa hóa đơn
                string deleteQuery = "DELETE FROM Invoice WHERE InvoiceID = @InvoiceID";
                SqlParameter[] parameters = { new SqlParameter("@InvoiceID", id) };
                da.ExecuteNonQuery(deleteQuery, parameters);

                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Lỗi khi xóa hóa đơn: " + ex.Message;
                return RedirectToAction("List"); // Hoặc trả về view khác nếu cần
            }
        }
    }
}