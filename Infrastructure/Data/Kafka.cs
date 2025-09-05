using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl.AdoJobStore.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class KafkaProducerFactory
    {
        private readonly ProducerConfig _config;

        public KafkaProducerFactory(ProducerConfig config)
        {
            _config = config;
        }

        public IProducer<TKey, TValue> GetProducer<TKey, TValue>(Func<ProducerBuilder<TKey, TValue>, IProducer<TKey, TValue>> builderConfig) => builderConfig(new ProducerBuilder<TKey, TValue>(_config));
    }
}
