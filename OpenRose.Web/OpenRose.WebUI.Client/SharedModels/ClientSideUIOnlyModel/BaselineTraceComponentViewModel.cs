namespace OpenRose.WebUI.Client.SharedModels.ClientSideUIOnlyModel
{
	public class BaselineTraceComponentViewModel
	{
		public Guid Id { get; set; }
		public string? Name { get; set; }
		public string? Status { get; set; }
		public string? Priority { get; set; }
		public bool isIncluded { get; set; }
		public string? TraceLabel { get; set; }
	}
}
