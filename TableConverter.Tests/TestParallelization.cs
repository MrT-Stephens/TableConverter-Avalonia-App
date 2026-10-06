using Xunit;

// ModelFlow keeps the way a data source reaches the UI thread in one place that the whole process shares,
// and a test that builds a data source has to put its own way there. Two test classes doing that at the
// same time leave one of them running against the other's way - which is a hook that outlives the test
// that set it - so the tests in this assembly are run one at a time rather than beside each other.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

