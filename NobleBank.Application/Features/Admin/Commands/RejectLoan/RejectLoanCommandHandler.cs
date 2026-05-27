using MediatR;
using Microsoft.EntityFrameworkCore;
using NobleBank.Application.Common.Exceptions;
using NobleBank.Application.Common.Interfaces;
using NobleBank.Domain.Common;
using NobleBank.Domain.Entities;

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
            {
                throw new UnauthorizedAccessException(Constants.Requirements.UserIdRequired);
            }

            Loan? loan = await _context.Loans
                .FirstOrDefaultAsync(l => l.Id == request.LoanId, cancellationToken);

            if (loan is null)
            {
                throw new NotFoundException(Constants.Exceptions.LoanNotFound);
            }

            if (loan.Status != LoansEnum.Status.Pending)
            {
                throw new DomainException($"Cannot reject loan in {loan.Status} status");
            }

            loan.Reject(request.Reason, request.AdminUserId);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
