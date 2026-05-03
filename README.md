## Introduction
The repository contains practical examples and exercises to help you move from a Junior/Middle level to a confident Middle/Senior Performance developer.

---

## Contents
### [1. Memory](#memory-solution)
- [Garbage Collector](#1-garbage-collector)
- [Zero Allocation](#2-zero-allocation)

### [2. Threading](#threading-solution)
- [ThreadPool](#1-threadpool-)
- [Synchronization primitives](#2-synchronization-primitives)
- [Signal primitives](#3-signal-primitives)

---

## Memory Solution
### 1. [Garbage Collector](./Memory/GarbageCollector)
- Full `IDisposable` + `Finalizer` pattern
- Event Bus pattern (`ConcurrentDictionary` + `WeakReference`)
- Working with unmanaged memory (`Marshal.AllocHGlobal`)
- Object Pooling for native buffers
- Memory Profiler and diagnostics. `GCHandle`, `Pinning`
- Finding and preventing memory leaks

### 2. [Zero Allocation](./Memory/ZeroAlloc)
- `stackalloc`, `Span<T>`, `ReadOnlySpan<T>`, `Memory<T>`, differences and use cases
- `ArrayPool<T>`, `MemoryPool<T>`, reducing allocations without GC pressure
- High-performance Binary Protocol Parser
- Zero-alloc Logger
- Image processing without heap allocations: `SIMD` and `unsafe` pointer arithmetic
- Each example includes a comparison of execution time and allocation count

## Threading Solution
### 1. [ThreadPool](./Threading/ThreadPool)
- Synchronization primitives: `lock`, `Monitor`, `Mutex`, `Semaphore`, `SemaphoreSlim`, `SpinLock`, `ReaderWriterLockSlim`
- `Producer-Consumer` pattern implemented in multiple ways using concurrent collections
- Custom `thread pool` implementation with `task scheduling`
- Low-level thread signaling using `Monitor.Wait`, `Pulse`

### 2. [Synchronization Primitives](./Threading/SynchronizationPrimitives)
- Patterns: `Multiple Readers One Writer`, `Single-flight`, `Double-Checked Locking`, `lazy initialization`
- `OrderBook`: `Multiple Readers One Writer` without any .NET built-ins, only `Monitor.Enter`, `Exit`, `Wait`, `PulseAll`
- `ConnectionPool`: bounded resource pool via custom `ManualSemaphore` built on `Monitor`, full `IDisposable` support
- `SmartCache<TKey, TValue>`: thread-safe cache with TTL, single-flight loading and background invalidation

### 3. [Signal Primitives](./Threading/SignalPrimitives)
- `MyAutoResetEvent`, `MyManualResetEvent`, `MyCountdownEvent`: built from scratch using only `Monitor.Wait`, `Pulse`, `PulseAll`
- `OrderPipeline`: pipeline `Validator` -> `Enricher` -> `Dispatcher` via `AutoResetEvent`
- `ConfigService`: config reload via `ManualResetEventSlim`
- `MapReduceRunner`: phased map-reduce barrier via `CountdownEvent`
