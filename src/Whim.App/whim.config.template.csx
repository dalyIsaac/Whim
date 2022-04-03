#r "WHIM_PATH\whim.dll"
#r "WHIM_PATH\plugins\Whim.Bar\Whim.Bar.dll"
#r "WHIM_PATH\plugins\Whim.FloatingLayout\Whim.FloatingLayout.dll"
#r "WHIM_PATH\plugins\Whim.Gaps\Whim.Gaps.dll"
#r "WHIM_PATH\plugins\Whim.TreeLayout\Whim.TreeLayout.dll"

using System;
using Whim;
using Whim.Bar;
using Whim.FloatingLayout;
using Whim.Gaps;
using Whim.TreeLayout;

Func<IConfigContext, IConfigContext> doConfig = (context) =>
{
	return context;
};

return doConfig;
