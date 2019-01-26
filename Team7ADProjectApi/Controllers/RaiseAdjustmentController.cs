﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Team7ADProjectApi.Entities;
using Team7ADProjectApi.ViewModels;

namespace Team7ADProjectApi.Controllers
{
    public class RaiseAdjustmentController : ApiController
    {
        public LogicDB context = new LogicDB();

        [HttpGet]
        [Route("adjustment/categories")]
        public IHttpActionResult getCategories()
        {
            List<string> results = context.Stationery.Select(x => x.Category).Distinct().ToList();
            return Ok(results);
        }

        [HttpGet]
        [Route("adjustment/items/{category}")]
        public IHttpActionResult getItems(string category)
        {
            List<Stationery> list = context.Stationery.Where(x => x.Category == category).ToList();
            List<AdjustmentItemDetail> results = new List<AdjustmentItemDetail>();
            foreach(Stationery item in list)
            {
                AdjustmentItemDetail resultItem = new AdjustmentItemDetail(item.ItemId,item.Category,item.Description,item.UnitOfMeasure,item.QuantityWarehouse,item.FirstSuppPrice);
                results.Add(resultItem);
            }

            return Ok(results);
        }

        [HttpGet]
        [Route("adjustment/item/{itemId}")]
        public IHttpActionResult getItem(string itemId)
        {
            Stationery item = context.Stationery.Single(x => x.ItemId == itemId);
            AdjustmentItemDetail result = new AdjustmentItemDetail(item.ItemId, item.Category, item.Description, item.UnitOfMeasure, item.QuantityWarehouse, item.FirstSuppPrice);
            return Ok(result);
        }

        [HttpPost]
        [Route("adjustment/save")]
        public IHttpActionResult save(List<AdjustmentInfo> request)
        {
            decimal? amount = 0;
            if (request != null)
            {
                StockAdjustment adjustment = new StockAdjustment
                {
                    StockAdjId = newAdjustId(),
                    PreparedBy = "0468dd23-2d20-4fae-b20c-8add2bce2d57",
                    ApprovedBy = null,
                    Remarks = "remark",
                    Date = DateTime.Today
                };

                foreach (var item in request)
                {
                    TransactionDetail transactionDetail = new TransactionDetail
                    {
                        TransactionId = GenerateTransactionDetailId(),
                        ItemId = item.itemId,
                        Quantity = item.quantity,
                        Remarks = "Pending Approval",
                        TransactionRef = adjustment.StockAdjId,
                        TransactionDate = DateTime.Today,
                        UnitPrice = context.Stationery.Single(x => x.ItemId == item.itemId).FirstSuppPrice
                    };
                    amount += item.quantity * transactionDetail.UnitPrice;
                    adjustment.TransactionDetail.Add(transactionDetail);
                    context.TransactionDetail.Add(transactionDetail);
                    context.SaveChanges();
                }

                context.StockAdjustment.Add(adjustment);
                context.SaveChanges();
                
            }
            return Ok();
        }

        public string newAdjustId()
        {
            string lastId = context.StockAdjustment.OrderByDescending(x => x.StockAdjId).FirstOrDefault().StockAdjId;
            if (lastId == null)
            {
                lastId = "SAD-000000";
            }
            int id = int.Parse(lastId.Substring(4));
            return "SAD-" + (id + 1).ToString().PadLeft(6, '0');
        }

        public int GenerateTransactionDetailId()
        {
            TransactionDetail lastItem = context.TransactionDetail.OrderByDescending(x => x.TransactionId).FirstOrDefault();
            int lastRequestId;
            if (lastItem == null)
            {
                lastRequestId = 1000;
            }
            else
            {
                lastRequestId = lastItem.TransactionId;
            }
            int newRequestId = lastRequestId + 1;
            return newRequestId;
        }
    }
}
