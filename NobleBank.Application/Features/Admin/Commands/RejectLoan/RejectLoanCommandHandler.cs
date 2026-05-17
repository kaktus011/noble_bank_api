using MediatR;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Domain.Common;

namespace NobleBank.Application.Features.Admin.Commands.RejectLoan
{
    public class RejectLoanCommandHandler : IRequestHandler<RejectLoanCommand>
    {
        private readonly IApplicationDbContext _context;

        public RejectLoanCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(RejectLoanCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.AdminUserId))
                throw new UnauthorizedAccessException(Constants.Exceptions.UserNotFound);

            var loan = await _context.Loans
                .FirstOrDefaultAsync(l => l.Id == request.LoanId, cancellationToken);

            if (loan is null)
            {
                throw new Exception("Loan not found");
            }

            if (loan.Status != LoansEnum.Status.Pending)
            {
                throw new Exception($"Cannot reject loan in {loan.Status} status");
            }

            loan.Reject(request.Reason, request.AdminUserId);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
