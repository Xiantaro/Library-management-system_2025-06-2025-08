using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using test2.Models.ManagementModels.GmailSMTP;
using test2.Models.ManagementModels.ZhongXian.Normal;

namespace test2.Models.ManagementModels.Services
{
    public class ScheduleServices : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFacotry;
        private readonly PeriodicTimer _timer;
        public ScheduleServices(IServiceScopeFactory scopeFacotry)
        {
            _scopeFacotry = scopeFacotry;
            _timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {   
            if(await _timer.WaitForNextTickAsync(stoppingToken))
            {
                Debug.WriteLine("排程開始!");
                using var scope = _scopeFacotry.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<Test2Context>();

                // 取書逾期 
                // 通知其他預約者、沒有預約者 
                OverdueClass OverdueClasss = new OverdueClass(context);
                MessageDTO overDueResult = await OverdueClasss.OverdueStart();
                Debug.WriteLine(overDueResult.Message);

                // 借閱逾期 更改狀態、Email+站內通知 
                LateRetrunClass LateRetrunClasss = new LateRetrunClass(context);
                var LateReturnResult = await LateRetrunClasss.LateRetrunStart();
                Debug.WriteLine(LateReturnResult);

                //即將逾期前一天、前三天 Email+站內通知 
                LateReturnOneToThreeClass LateReturnBefore = new LateReturnOneToThreeClass(context);
                var LateReturn2 = await LateReturnBefore.LateReturnStartOnToThree();
                Debug.WriteLine(LateReturn2);

                Debug.WriteLine("排程結束.........");
            }
            _timer.Dispose();
        }
        public override void Dispose()
        {
            _timer.Dispose();
            base.Dispose();
        }
    }
}
