using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using YammerlyService.DataObjects;
using YammerlyService.Models;
using Yammerly.Service.Helpers;

namespace YammerlyService.Controllers
{
    [Authorize]
    public class TimelineItemController : TableController<TimelineItem>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<TimelineItem>(context, Request);
        }

        // GET tables/TimelineItem
        [ExpandProperty("Author")]
        public IQueryable<TimelineItem> GetAllTimelineItem()
        {
            return Query(); 
        }

        [ExpandProperty("Author")]
        // GET tables/TimelineItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<TimelineItem> GetTimelineItem(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/TimelineItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<TimelineItem> PatchTimelineItem(string id, Delta<TimelineItem> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/TimelineItem
        public async Task<IHttpActionResult> PostTimelineItem(TimelineItem item)
        {
            TimelineItem current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/TimelineItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteTimelineItem(string id)
        {
             return DeleteAsync(id);
        }
    }
}
