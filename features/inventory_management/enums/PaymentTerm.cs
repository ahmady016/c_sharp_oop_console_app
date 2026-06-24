namespace InventoryManagement;

public enum PaymentTerm : byte
{
    Net30 = 1,
    Net60 = 2,
    PayPal = 3,
    Stripe = 4,
    Cash = 5,
    Credit = 6,
    Other = 7
}
