using MultiTenantApp.Web.Models.DTOs;
using System;
using System.Threading.Tasks;

namespace MultiTenantApp.Web.Interfaces
{
    public interface IFinanceService
    {
        Task<FinanceDto?> GetByIdAsync(Guid id);
        Task<PagedResponse<FinanceListDto>> GetAllAsync(PagedRequest request);
        Task<FinanceDto> RegisterPayment(CreateFinanceDto model);
        Task<FinanceSummaryDto> GetSummary(DateTime? start = null, DateTime? end = null);
        Task<bool> MonthlyClosing(int month, int year);
    }
}
