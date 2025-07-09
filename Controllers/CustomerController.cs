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
    public class CustomerController : Controller
    {
        private DataAccess da = new DataAccess();

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            DataTable dt = da.ExecuteQuery("SELECT * FROM Customer");
            return View(dt);
        }

        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(Customer model)
        {
            if (ModelState.IsValid)
            {
                string checkQuery = "SELECT COUNT(*) FROM Customer WHERE CustomerID = @CustomerID";
                SqlParameter[] checkParams = { new SqlParameter("@CustomerID", model.CustomerID.Trim().ToUpper()) };
                DataTable dt = da.ExecuteQuery(checkQuery, checkParams);

                if (Convert.ToInt32(dt.Rows[0][0]) > 0)
                {
                    ViewBag.Message = "Mã khách hàng đã tồn tại!";
                    return View(model);
                }

                string insertQuery = "INSERT INTO Customer (CustomerID, CustomerName, Phone) VALUES (@CustomerID, @CustomerName, @Phone)";
                SqlParameter[] insertParams = {
                    new SqlParameter("@CustomerID", model.CustomerID.Trim().ToUpper()),
                    new SqlParameter("@CustomerName", model.CustomerName),
                    new SqlParameter("@Phone", model.Phone)
                };
                da.ExecuteNonQuery(insertQuery, insertParams);
                return RedirectToAction("List");
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            string query = "SELECT * FROM Customer WHERE CustomerID = @CustomerID";
            SqlParameter[] parameters = { new SqlParameter("@CustomerID", id) };
            DataTable dt = da.ExecuteQuery(query, parameters);
            if (dt.Rows.Count > 0)
            {
                Customer customer = new Customer
                {
                    CustomerID = dt.Rows[0]["CustomerID"].ToString(),
                    CustomerName = dt.Rows[0]["CustomerName"].ToString(),
                    Phone = dt.Rows[0]["Phone"].ToString()
                };
                return View(customer);
            }
            return RedirectToAction("List");
        }

        [HttpPost]
        public ActionResult Edit(Customer model)
        {
            if (ModelState.IsValid)
            {
                string updateQuery = "UPDATE Customer SET CustomerName = @CustomerName, Phone = @Phone WHERE CustomerID = @CustomerID";
                SqlParameter[] updateParams = {
                    new SqlParameter("@CustomerName", model.CustomerName),
                    new SqlParameter("@Phone", model.Phone),
                    new SqlParameter("@CustomerID", model.CustomerID)
                };
                da.ExecuteNonQuery(updateQuery, updateParams);
                return RedirectToAction("List");
            }
            return View(model);
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            // Kiểm tra xem khách hàng có được sử dụng trong hóa đơn không
            string checkQuery = "SELECT COUNT(*) FROM Invoice WHERE CustomerID = @CustomerID";
            SqlParameter[] checkParams = { new SqlParameter("@CustomerID", id) }; // Tạo mới mảng tham số
            DataTable dt = da.ExecuteQuery(checkQuery, checkParams);

            if (Convert.ToInt32(dt.Rows[0][0]) > 0)
            {
                ViewBag.Message = "Khách hàng đã được sử dụng trong hóa đơn, không thể xóa!";
                return RedirectToAction("List");
            }

            // Thực hiện xóa
            string deleteQuery = "DELETE FROM Customer WHERE CustomerID = @CustomerID";
            SqlParameter[] deleteParams = { new SqlParameter("@CustomerID", id) }; // Tạo mới mảng tham số
            da.ExecuteNonQuery(deleteQuery, deleteParams);
            return RedirectToAction("List");
        }
    }
}