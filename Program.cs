using CodeAegis.Configuration;
using CodeAegis.Services;

// 1. Build the Kernel using our centralized configuration
var kernel = KernelSetup.BuildLocalKernel();

// 2. Instantiate the Orchestrator
var agent = new CodeReviewAgent(kernel);

// 3. Execute the workflow (Change the path to match your actual FakeRepo path!)
await agent.RunDirectoryAuditAsync("/Users/gauthamsreedhar/Desktop/Projects/CodeAegis");