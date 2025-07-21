namespace CoporateBooking.Controllers.common
{
    internal class CancelStatusRequest
    {
        private string recordLocator;
        private int status;
        private string userEmail;
        private decimal balanceDue;
        private decimal totalAmt;

        public CancelStatusRequest(string recordLocator, int status, string userEmail, decimal balanceDue, decimal totalAmt)
        {
            this.recordLocator = recordLocator;
            this.status = status;
            this.userEmail = userEmail;
            this.balanceDue = balanceDue;
            this.totalAmt = totalAmt;
        }
    }
}