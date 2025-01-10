// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using OpenRose.WebUI.Client.SharedModels;

namespace OpenRose.WebUI.Client.Services.ItemzTrace
{
    public class DummyItemzTraceService : IItemzTraceService
    {
        public Task __DELETE_AllFromItemz_Trace__Async(Guid itemzID)
        {
            throw new NotImplementedException();
        }

        public Task __DELETE_AllToItemz_Trace__Async(Guid itemzID)
        {
            throw new NotImplementedException();
        }

        public Task __DELETE_Itemz_Trace_Collection__Async(IEnumerable<ItemzTraceDTO> body)
        {
            throw new NotImplementedException();
        }

        public Task __DELETE_Itemz_Trace__Async(ItemzTraceDTO body)
        {
            throw new NotImplementedException();
        }

        public Task<int> __GET_All_From_and_To_Itemz_Traces_Count_By_ItemzID__Async(Guid itemzId)
        {
            throw new NotImplementedException();
        }

        public Task<ItemzParentAndChildTraceDTO> __GET_All_Parent_and_Child_Itemz_Traces_By_ItemzID__Async(Guid itemzId)
        {
            throw new NotImplementedException();
        }

        public Task<ItemzTraceDTO> __GET_Check_Itemz_Trace_Exists__Async(Guid? fromTraceItemzId, Guid? toTraceItemzId)
        {
            throw new NotImplementedException();
        }

        public Task<int> __GET_FromItemz_Count_By_ItemzID__Async(Guid itemzId)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<ItemzTraceDTO>> __GET_Itemz_Traces_By_ItemzID__Async(Guid itemzId)
        {
            throw new NotImplementedException();
        }

        public Task<int> __GET_ToItemz_Count_By_ItemzID__Async(Guid itemzId)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<ItemzTraceDTO>> __POST_Create_Or_Verify_Itemz_Trace_Collection__Async(IEnumerable<ItemzTraceDTO> body)
        {
            throw new NotImplementedException();
        }

        public Task<ItemzTraceDTO> __POST_Establish_Trace_Between_Itemz__Async(ItemzTraceDTO body)
        {
            throw new NotImplementedException();
        }
    }
}
