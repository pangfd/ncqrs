﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using Ncqrs.Domain;
using Ncqrs.Domain.Storage;
using NUnit.Framework;

namespace Ncqrs.Tests.Domain.Storage
{
    public class DefaultAggregateRootLoaderTests
    {
        public class FooEvent : DomainEvent
        { }

        public class BarEvent : DomainEvent
        { }

        public class MyAggregateRoot : AggregateRootMappedByConvention
        {
            public List<DomainEvent> AppliedEvents = new List<DomainEvent>();

            protected void OnFooEvent(FooEvent e)
            {
                AppliedEvents.Add(e);
            }

            protected void OnBarEvent(BarEvent e)
            {
                AppliedEvents.Add(e);
            }
        }

        public class AggregateRootWithoutDefaultCtor : AggregateRoot
        {
            public AggregateRootWithoutDefaultCtor(String aParameter)
            {
                
            }
        }

        [Test]
        public void Should_throw_exception_when_aggregate_root_type_is_null()
        {
            Type aggregateRootType = null;
            IEnumerable<DomainEvent> events = new DomainEvent[] { new FooEvent(), new BarEvent() };

            var loader = new DefaultAggregateRootLoader();

            Action act = () => loader.LoadAggregateRootFromEvents(aggregateRootType, events);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Should_throw_exception_when_events_is_null()
        {
            Type aggregateRootType = typeof(MyAggregateRoot);
            IEnumerable<DomainEvent> events = null;

            var loader = new DefaultAggregateRootLoader();

            Action act = () => loader.LoadAggregateRootFromEvents(aggregateRootType, events);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Load_from_events_should_call_apply_event_for_all_events()
        {
            Type aggregateRootType = typeof(MyAggregateRoot);
            IEnumerable<DomainEvent> events = new DomainEvent[] { new FooEvent(), new BarEvent(), 
                                                                  new BarEvent(), new BarEvent(), 
                                                                  new FooEvent(), new BarEvent(), 
                                                                  new FooEvent(), new FooEvent() };

            var loader = new DefaultAggregateRootLoader();

            MyAggregateRoot loadedAggregateRoot = (MyAggregateRoot)loader.LoadAggregateRootFromEvents(aggregateRootType, events);

            loadedAggregateRoot.AppliedEvents.Should().ContainInOrder(events);
        }

        [Test]
        public void Load_from_events_should_throw_an_exception_when_target_does_not_contain_a_default_ctor()
        {
            Type aggregateRootType = typeof(AggregateRootWithoutDefaultCtor);
            IEnumerable<DomainEvent> events = new DomainEvent[] {new FooEvent(), new BarEvent()};

            var loader = new DefaultAggregateRootLoader();

            Action act = ()=> loader.LoadAggregateRootFromEvents(aggregateRootType, events);

            act.ShouldThrow<AggregateLoaderException>().And.Message.Should().Contain("No constructor found on aggregate root");
        }
    }
}
