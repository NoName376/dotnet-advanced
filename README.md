The repository contains practical examples and exercises to help you move from a Junior/Middle level to a confident Middle/Senior Performance developer.


## Memory Solution
### 1. Garbage Collector
- Full IDisposable + Finalizer pattern
- Event Bus pattern (ConcurrentDictionary + WeakReference)
- Working with unmanaged memory (Marshal.AllocHGlobal)
- Object Pooling for native buffers
- Finding and preventing memory leaks
- GCHandle, Pinning
- Memory Profiler and diagnostics

### 2. Zero Allocation Programming
- "stackalloc" "Span<T>", "ReadOnlySpan<T>", "Memory<T>" differences and use cases
- "ArrayPool<T>", "MemoryPool<T>" use to reduce allocations
- High-performance Binary Protocol Parser
- Zero-alloc Logger
- Image processing without heap allocations, (SIMD, and usafe pointer arithmetic)
- The examples provide a comparison and analysis of execution time and the amount of allocations.

## TPL 
### 1. ThreadPool 
- Interlock and Synchronization Primitives: "lock", "Monitor", "Mutex", "Semaphore", "SemaphoreSlim", "SpinLock", and "ReaderWriterLockSlim".
- Implementing the Producer-Consumer pattern in different ways and using concurrent collections
- Implementing a custom thread pool and understanding task scheduling.
- Low-level thread signaling using "Monitor.Wait"/"Pulse"

### 2. SynchronizationPrimitives 
- Multiple Readers, One Writer and Single-flight, Double-Checked Locking, lazy init patterns
- OrderBook implements the multiple Readers, one Writer pattern without using ready-made .net solutions. Only Monitor Enter, Exit, Wait, PulseAll
- ConnectionPool using own ManualSemaphore via Monitor without Semaphore/SemaphoreSlim also using the Disposable pattern
- SmartCache<TKey, TValue> is a thread-safe cache with TTL and lazy loading. Single-flight loading and background invalidation by TTL.