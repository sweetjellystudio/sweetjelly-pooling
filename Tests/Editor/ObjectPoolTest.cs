namespace SWeetJelly.Pooling.Tests.EditMode
{
    using System;
    using NUnit.Framework;
    using SWeetJelly.Pooling.Domain;

    public sealed class ObjectPoolTest
    {
        [Test]
        public void Constructor_WithInitialSize_PrewarmsInactiveItems()
        {
            var created = 0;

            var pool = new ObjectPool<SampleItem>(
                create: () => new SampleItem(++created),
                initialSize: 3,
                maxSize: 5);

            Assert.That(pool.Count, Is.EqualTo(3));
            Assert.That(pool.ActiveCount, Is.EqualTo(0));
            Assert.That(pool.InactiveCount, Is.EqualTo(3));
            Assert.That(created, Is.EqualTo(3));
        }

        [Test]
        public void RentAndReturn_ReusesReturnedItem()
        {
            var pool = new ObjectPool<SampleItem>(
                create: () => new SampleItem(1),
                onRent: item => item.IsActive = true,
                onReturn: item => item.IsActive = false,
                initialSize: 1,
                maxSize: 1);

            var first = pool.Rent();

            Assert.That(first.IsActive, Is.True);
            Assert.That(pool.ActiveCount, Is.EqualTo(1));

            pool.Return(first);

            Assert.That(first.IsActive, Is.False);
            Assert.That(pool.InactiveCount, Is.EqualTo(1));

            var second = pool.Rent();

            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void TryRent_WhenMaxSizeReached_ReturnsFalse()
        {
            var pool = new ObjectPool<SampleItem>(
                create: () => new SampleItem(1),
                initialSize: 1,
                maxSize: 1);

            Assert.That(pool.TryRent(out var first), Is.True);
            Assert.That(first, Is.Not.Null);
            Assert.That(pool.TryRent(out var second), Is.False);
            Assert.That(second, Is.Null);
        }

        [Test]
        public void Return_WithForeignItem_ThrowsInvalidOperationException()
        {
            var pool = new ObjectPool<SampleItem>(
                create: () => new SampleItem(1),
                initialSize: 1);

            Assert.Throws<InvalidOperationException>(() =>
                pool.Return(new SampleItem(99)));
        }

        private sealed class SampleItem
        {
            public SampleItem(int id)
            {
                Id = id;
            }

            public int Id { get; }
            public bool IsActive { get; set; }
        }
    }
}
