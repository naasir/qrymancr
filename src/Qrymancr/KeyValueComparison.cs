namespace Qrymancr
{
    using System.Linq;

    /// <summary>
    /// Represents a key/value comparison
    /// </summary>
    public class KeyValueComparison
    {
        /// <summary>
        /// List of supported comparison operators
        /// </summary>
        private readonly char[] operators = new[] { '=', '!', '<', '>', '^', '$', '*' };

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueComparison"/> class.
        /// </summary>
        /// <param name="augmentedKey">The augmented key.</param>
        /// <param name="value">The value.</param>
        public KeyValueComparison(string augmentedKey, string value)
        {
            this.Value = value;

            var lastChar = augmentedKey.ToCharArray().Last();
            this.Operator = '=';
            if (this.operators.Contains(lastChar))
            {
                // strip away operator
                augmentedKey = augmentedKey.Substring(0, augmentedKey.Length - 1);
                this.Operator = lastChar;
            }

            // interpret hyphenated keys as nested properties.
            // for example, the key 'template-name' will be interpreted as template.name
            this.Key = augmentedKey.Replace('-', '.');
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Gets the comparison operator.
        /// </summary>
        public char Operator { get; private set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string format)
        {
            return string.Format(format, this.Key, this.Value, this.Operator);
        }
    }
}