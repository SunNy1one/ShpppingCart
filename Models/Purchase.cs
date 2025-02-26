﻿namespace ShoppingCart.Models
{
    public class PurchaseCart
    {
        public PurchaseCart(string username)
        {
            this.username = username;
        }

        public PurchaseCart()
        {

        }
        public string? username { get; set; }

        public List<Software> softwareList = new List<Software>();

        public int GetSoftwareCount(string softwareId)
        {
            return softwareList.Where((s) => s.softwareId == softwareId).Count();
        }

        public ISet<Software> softwareToPurchase()
        {
            ISet<string> set = softwareList.Select((s) => s.softwareId).ToHashSet<string>();
            return softwareList.Where((s) => set.Contains(s.softwareId)).ToHashSet();
        }

        public ISet<string> softwareIdsToPurchase()
        {
            return softwareList.Select((s) => s.softwareId).ToHashSet<string>();
        }

        public int GetPurchaseCartPrice()
        {
            double totalamount = 0;
            foreach (Software software in softwareList)
            {
                totalamount += software.price;

            }
            return (int)(double)totalamount;

        }
    }
    public class Purchase
    {
        public string purchaseId { get; set; }

        public List<PurchaseUnit> purchaseUnits = new List<PurchaseUnit>();
        public string userId { get; set; }
        public DateTime dateOfPurchase { get; set; }

        public PurchaseStatus status { get; set; } = PurchaseStatus.Pending;

        public Purchase(string id, string userId, DateTime dateOfPurchase, List<PurchaseUnit> units)
        {
            this.purchaseId = id;
            this.userId = userId;
            this.dateOfPurchase = dateOfPurchase;
            this.purchaseUnits = units;
        }

        public void AddUnit(string softwareId)
        {
            purchaseUnits.Add(new PurchaseUnit(this.purchaseId, softwareId, buildActivationCode()));
        }

        public void CompletePurchase()
        {
            status = PurchaseStatus.Completed;
        }

        public string buildActivationCode()
        {
            return Guid.NewGuid().ToString();
        }

        public class PurchaseUnit
        {
            public string purchaseId { get; set; }
            public string softwareId { get; set; }
            public string activationCode { get; set; }

            public PurchaseUnit(string purchaseId, string softwareId, string activationCode)
            {
                this.purchaseId = purchaseId;
                this.softwareId = softwareId;
                this.activationCode = activationCode;
            }
        }
    }

    public enum PurchaseStatus
    {
        Pending, Canceled, Completed
    }





    public class PurchaseDTO
    {
        public string softwareid { get; set; }
        public Software software { get; set; }
        public DateTime lastdateOfPurchase { get; set; }

        public List<string> activationcodeList =new List<string>();


    }

}
