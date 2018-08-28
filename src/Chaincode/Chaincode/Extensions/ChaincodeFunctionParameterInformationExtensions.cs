using System;
using System.Globalization;
using Thinktecture.HyperledgerFabric.Chaincode.Chaincode;

namespace Thinktecture.HyperledgerFabric.Chaincode.Extensions
{
    public static class ChaincodeFunctionParameterInformationExtensions
    {
        /// <summary>
        /// Returns the given <see cref="Parameters" /> <paramref name="index" /> converted to <see cref="T" />.
        /// </summary>
        /// <param name="info">An instance of <see cref="ChaincodeFunctionParameterInformation" /></param>
        /// <param name="index">The numeric index to get.</param>
        /// <typeparam name="T">Result type</typeparam>
        /// <returns>Returns the value of the given <see cref="index" /></returns>
        public static T Get<T>(this ChaincodeFunctionParameterInformation info, int index)
        {
            return info.Parameters.Get<T>(index);
        }

        /// <summary>
        /// Returns the given <paramref name="index" /> converted to <see cref="T" />.
        /// </summary>
        /// <param name="parameters">An instance of <see cref="Parameters" /></param>
        /// <param name="index">The numeric index to get.</param>
        /// <typeparam name="T">Result type</typeparam>
        /// <returns>Returns the value of the given <see cref="index" /></returns>
        public static T Get<T>(this Parameters parameters, int index)
        {
            if (index < 0) throw new ArgumentException("index can not be less than zero", nameof(index));

            if (index > parameters.Count) return default; // TODO: or throw? 

            var value = parameters[index];

            if (value == null) // TODO: can this really happen?
                throw new InvalidCastException($"Can not cast null value to {typeof(T)}");

            if (value is T t) return t;

            return (T) Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Tries to return the given <see cref="Parameters" /> <paramref name="index" /> converted to <see cref="T" />.
        /// </summary>
        /// <param name="info">An instance of <see cref="ChaincodeFunctionParameterInformation" /></param>
        /// <param name="index">The numeric index to get.</param>
        /// <param name="convertedValue">The converted value</param>
        /// <typeparam name="T">Result type</typeparam>
        /// <returns>True, if conversion was successful. Otherwise false.</returns>
        public static bool TryGet<T>(
            this ChaincodeFunctionParameterInformation info,
            int index,
            out T convertedValue
        )
        {
            return info.Parameters.TryGet(index, out convertedValue);
        }

        /// <summary>
        /// Tries to return the given <see cref="Parameters" /> <paramref name="index" /> converted to <see cref="T" />.
        /// </summary>
        /// <param name="parameters">An instance of <see cref="Parameters" /></param>
        /// <param name="index">The numeric index to get.</param>
        /// <param name="convertedValue">The converted value</param>
        /// <typeparam name="T">Result type</typeparam>
        /// <returns>True, if conversion was successful. Otherwise false.</returns>
        public static bool TryGet<T>(this Parameters parameters, int index, out T convertedValue)
        {
            convertedValue = default;

            try
            {
                convertedValue = parameters.Get<T>(index);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
