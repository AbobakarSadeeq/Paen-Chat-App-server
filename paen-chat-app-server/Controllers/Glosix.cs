using DataAccess.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace paen_chat_app_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Glosix : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> GstCalculator(GstCalculate gstCalculate)
        {
            // find the percentage of product
            int gstAmountWithIncludeProduct = (gstCalculate.ProductPrice * gstCalculate.GSTPercentage)
                / 100;

            int removeGSTFromProduct = gstCalculate.ProductPrice - (gstCalculate.ProductPrice * (100 / (100 + gstCalculate.GSTPercentage)));


            return Ok(new
            {
                GstAmount = "GST Amount is: " + gstAmountWithIncludeProduct + "%",
                ProductOriginalPrice = "Product Original Price is: Rs" + removeGSTFromProduct,
                GstIncludeInProductTotal = "GST include in product price is: Rs" + (gstAmountWithIncludeProduct + removeGSTFromProduct)
            });
        }

        [HttpPost("{MonthlyBillingCalculator}")]
        public async  Task<IActionResult> MonthlyBillingCalculator(MonthlyBiling  monthlyBiling)
        {
            DateTime rentalStartDate = DateTime.Parse(monthlyBiling.RentalStart);
            DateTime rentalEndDate = DateTime.Parse(monthlyBiling.RentalEnd);
            int differenceBetweenRentalStartMonthAndEnd = rentalEndDate.Month - rentalStartDate.Month + 1;
            int totalRentSum = 0;
            int refundAmount = 0;

            if (rentalStartDate.Day == rentalEndDate.Day)
            {
             totalRentSum = differenceBetweenRentalStartMonthAndEnd * monthlyBiling.SingleMonthRent;

            }else
            {
                 // if last month is not complete
               // int totalDaysUsedServices = rentalEndDate.Day - rentalStartDate.Day;
                int priceEachDayPaying = monthlyBiling.SingleMonthRent / 30;
                int SingleMonthDays = DateTime.DaysInMonth(rentalEndDate.Year, rentalEndDate.Month);
                int remainsDaysToGiveRefundIs = SingleMonthDays - rentalEndDate.Day;
                refundAmount = remainsDaysToGiveRefundIs * priceEachDayPaying;
                totalRentSum = differenceBetweenRentalStartMonthAndEnd * monthlyBiling.SingleMonthRent;
                totalRentSum = totalRentSum - refundAmount;
            }

            List<MonthsAndPayment> result = new List<MonthsAndPayment>();
            int monthIncrese = rentalStartDate.Month;
            for (int i = 0; i < differenceBetweenRentalStartMonthAndEnd; i++)
            {
                
                if(rentalEndDate.Month != rentalStartDate.Month + 1)
                {

                   
                        result.Add(new MonthsAndPayment()
                        {
                            Months = rentalStartDate.ToString("dd-MMM") + " to " + (rentalStartDate.Month+ monthIncrese).ToString("dd-MMM"),
                            RentalPrice = monthlyBiling.SingleMonthRent
                        });
                   
                }else if(rentalEndDate.Month == rentalStartDate.Month + 1)
                {
                    result.Add(new MonthsAndPayment()
                    {
                        Months = rentalStartDate.ToString("dd-MMM") + " to " + rentalEndDate.ToString("dd-MMM"),
                       RentalPrice = refundAmount
                   });
                }


                monthIncrese++;
        


            }
            return Ok(
                new
                {
                    totalRent = differenceBetweenRentalStartMonthAndEnd * monthlyBiling.SingleMonthRent,
                    RefundAmount = refundAmount,
                    MonthlyRentList = result

                }
                
                );
        }


    }
}
