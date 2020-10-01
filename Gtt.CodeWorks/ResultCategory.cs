namespace Gtt.CodeWorks
{
    /// <summary>
    /// Service result was successful or failed for one of these reasons
    /// </summary>
    public enum ResultCategory
    {
        /// <summary>
        /// Service result failed due to a business or logic error
        /// </summary>
        BusinessLogicError,
        /// <summary>
        /// Service result failed due to a system error
        /// </summary>
        SystemError,
        /// <summary>
        /// Service result failed due to an upstream service error
        /// </summary>
        DependencyError,
        /// <summary>
        /// Service result was successful
        /// </summary>
        Successful
    }
}