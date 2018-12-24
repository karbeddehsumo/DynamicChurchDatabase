[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(WebUI.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(WebUI.App_Start.NinjectWebCommon), "Stop")]

namespace WebUI.App_Start
{
    using System;
    using System.Web;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using System.Web.Routing;
    using System.Web.Mvc;
    using System.Linq;
    using Ninject;
    using Ninject.Web.Common;
    using Ninject.Web.Mvc;
    using Domain.Abstract;
    using Domain.Concrete;
 


    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        /// <summary>
        
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IActionItemRepository>().To<EFActionItemRepository>();
            kernel.Bind<IAnnouncementRepository>().To<EFAnnouncementRepository>();
            kernel.Bind<IAttendanceRepository>().To<EFAttendanceRepository>();
            kernel.Bind<IBankAccountRepository>().To<EFBankAccountRepository>();
            kernel.Bind<IBankBalanceRepository>().To<EFBankBalanceRepository>();
            kernel.Bind<IBillRepository>().To<EFBillRepository>();
            kernel.Bind<IBudgetItemRepository>().To<EFBudgetItemRepository>();
            kernel.Bind<IBudgetRepository>().To<EFBudgetRepository>();
            kernel.Bind<ICalendarRepository>().To<EFCalendarRepository>();
            kernel.Bind<ICategoryRepository>().To<EFCategoryRepository>();
            kernel.Bind<IChildParentRepository>().To<EFChildParentRepository>();
            kernel.Bind<IConstantRepository>().To<EFConstantRepository>();
            kernel.Bind<IContributionRepository>().To<EFContributionRepository>();
            kernel.Bind<IDocumentRepository>().To<EFDocumentRepository>();
            

            kernel.Bind<IExpenseRepository>().To<EFExpenseRepository>();
            kernel.Bind<IMinistryExpenseRepository>().To<EFMinistryExpenseRepository>();
            kernel.Bind<IFamilyRepository>().To<EFFamilyRepository>();
            kernel.Bind<IGoalRepository>().To<EFGoalRepository>();
            kernel.Bind<IIncomeRepository>().To<EFIncomeRepository>();
            kernel.Bind<IMinistryIncomeRepository>().To<EFMinistryIncomeRepository>();

            kernel.Bind<IListItemRepository>().To<EFListItemRepository>();
            kernel.Bind<IListtableRepository>().To<EFListTableRepository>();
            kernel.Bind<IListHeaderRepository>().To<EFListHeaderRepository>();

            kernel.Bind<IMeetingAgendaRepository>().To<EFMeetingAgendaRepository>();
            kernel.Bind<IMeetingNotesRepository>().To<EFMeetingNotesRepository>();
            kernel.Bind<IMeetingRepository>().To<EFMeetingRepository>();

            kernel.Bind<IMemberRepository>().To<EFMemberRepository>();
            kernel.Bind<IMinistryGroupRepository>().To<EFMinistryGroupRepository>();
            kernel.Bind<IMinistryMemberRepository>().To<EFMinistryMemberRepository>();

            kernel.Bind<IMinistryRepository>().To<EFMinistryRepository>();
            kernel.Bind<IPayeeCategoryRepository>().To<EFPayeeCategoryRepository>();
            kernel.Bind<IPayeeRepository>().To<EFPayeeRepository>();

            kernel.Bind<IPictureRepository>().To<EFPictureRepository>();
            kernel.Bind<IPledgeRepository>().To<EFPledgeRepository>();

            kernel.Bind<IProductDiscountRepository>().To<EFProductDiscountRepository>();
            kernel.Bind<IProductRepository>().To<EFProductRepository>();

           
            kernel.Bind<IProgramEventRepository>().To<EFProgramEventRepository>();
            kernel.Bind<IPropertyRepository>().To<EFPropertyRepository>();
            kernel.Bind<IPropertyInventoryRepository>().To<EFPropertyInventoryRepository>();
            kernel.Bind<IProgramEventBudgetRepository>().To<EFProgramEventBudgetRepository>();

            kernel.Bind<IResponsibilityRepository>().To<EFResponsibilityRepository>();
            kernel.Bind<ISaleItemRepository>().To<EFSaleItemRepository>();
            kernel.Bind<ISaleRepository>().To<EFSaleRepository>();
            kernel.Bind<ISpouseRepository>().To<EFSpouseRepository>();
            kernel.Bind<IStaffRepository>().To<EFStaffRepository>();
            kernel.Bind<IStoryRepository>().To<EFStoryRepository>();
            kernel.Bind<ISubCategoryItemRepository>().To<EFSubCategoryItemRepository>();

            kernel.Bind<ISubCategoryRepository>().To<EFSubCategoryRepository>();
            kernel.Bind<ISupervisorRepository>().To<EFSupervisorRepository>();
            kernel.Bind<ITaskRepository>().To<EFTaskRepository>();
            kernel.Bind<IVideoRepository>().To<EFVideoRepository>();
            kernel.Bind<IVisitorRepository>().To<EFVisitorRepository>();


            
            
            
        }        
    }
}
