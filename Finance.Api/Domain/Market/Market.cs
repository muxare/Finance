using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Finance.Api.Domain.Market
{
    public class Market
    {
        public IEnumerable<Holding> Holdings { get; set; }
        public Accout Account { get; set; }

        public Market()
        {
        }

        public void Buy(Holding holding)
        {

        }

        public void Sell(Holding holding)
        {

        }

    }
}
