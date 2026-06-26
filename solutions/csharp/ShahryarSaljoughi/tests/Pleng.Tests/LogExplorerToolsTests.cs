using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Pleng.MCP.Server.Tools;
using Pleng.MCP.Server.Services;
using ModelContextProtocol;

namespace Pleng.Tests
{
    public class LogExplorerToolsTests
    {
        [Fact]
        public void GetServiceNames_Returns_Registered_Services()
        {
            var adapter = new LogStorageAdapter();
            var tools = new LogExplorerTools(adapter);

            var services = tools.GetServiceNames();

            Assert.NotNull(services);
            Assert.Contains("Payment", services, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetLogsAsync_Returns_Logs_For_Valid_Service()
        {
            var adapter = new LogStorageAdapter();
            var tools = new LogExplorerTools(adapter);

            var logs = await tools.GetLogsAsync("Payment", 60);

            Assert.NotNull(logs);
            Assert.NotEmpty(logs);
            Assert.All(logs, s => Assert.Contains("Payment", s, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetLogsAsync_InvalidInputs_Throws()
        {
            var adapter = new LogStorageAdapter();
            var tools = new LogExplorerTools(adapter);

            await Assert.ThrowsAsync<McpException>(async () => await tools.GetLogsAsync("", 15));
            await Assert.ThrowsAsync<McpException>(async () => await tools.GetLogsAsync("UnknownService", 15));
            await Assert.ThrowsAsync<McpException>(async () => await tools.GetLogsAsync("Payment", 0));
        }
    }
}
