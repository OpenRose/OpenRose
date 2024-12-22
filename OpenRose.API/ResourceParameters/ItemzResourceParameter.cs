// OpenRose - Requirements Management
// Licensed under the Apache License, Version 2.0. 
// See the LICENSE file or visit https://github.com/OpenRose/OpenRose for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ItemzApp.API.ResourceParameters
{
    public class ItemzResourceParameter
    {
        const int maxPageSize = 100;
        public int PageNumber { get; set; } = 1;
        private int _pageSize = 10;
        public int PageSize {
            get => _pageSize;
            // EXPLANATION: in following set statement, because we are using short hand 
            // for Expression-Bodied Property Accessors, 
            // by using "set =>" at the start of the statement. 
            // Checkout https://csharp.christiannagel.com/2017/01/25/expressionbodiedmembers/
            // to see a simple example.
            set => _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }

        public string OrderBy { get; set; } = "Name";
    }
}
