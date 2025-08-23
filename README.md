# p42ObjectStores

Lightweight helpers to access simple blob/object stores (AWS S3, DigitalOcean Spaces, etc.) from .NET.

- PackageId: `p42ObjectStores`
- Version: `1.0.0`
- Target framework: .NET 9.0
- Language: C# (modern)
- License: MIT
- Repository: https://github.com/prod42net/p42ObjectStores

## Overview

p42ObjectStores provides small, focused utilities to simplify working with object/blob stores (for example: AWS S3 or
S3-compatible hosts such as DigitalOcean Spaces). It is intended to be lightweight and to integrate easily with existing
.NET applications.

This library depends on the official AWS S3 SDK for .NET and a shared internal base library used across related
projects.

## Features

- Simplified access patterns for object stores
- Thin wrapper helpers around S3-compatible APIs
- Designed to be small and easy to drop into existing projects

## Requirements

- .NET 9.0 SDK
- C# 13.0 compatible compiler
- AWSSDK.S3 package
- p42BaseLib (project reference used for shared utilities)

## Installation

If a NuGet package is published, you would install it with:

dotnet add package p42ObjectStores --version 1.0.0

Or add the project to your solution and reference the project directly.

## Quick Start

Below is a minimal illustrative example showing the general idea of usage with an S3-compatible client. Adapt to your
concrete API surface provided by this library.
