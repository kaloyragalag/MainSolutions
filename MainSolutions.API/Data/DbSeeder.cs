using MainSolutions.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MainSolutions.API.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        await SeedUsersAsync(context);
        await SeedCategoriesAndProductsAsync(context);
    }

    private static async Task SeedUsersAsync(AppDbContext context)
    {
        if (!await context.Users.AnyAsync(u => u.Email == "admin@mainsolutions.com"))
        {
            context.Users.Add(new User
            {
                Email = "admin@mainsolutions.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FirstName = "Admin",
                LastName = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
            await context.SaveChangesAsync();
            Console.WriteLine("Seed: Admin user created.");
        }
        else
        {
            Console.WriteLine("Seed: Admin user already exists, skipping.");
        }
    }

    private static async Task SeedCategoriesAndProductsAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync())
        {
            Console.WriteLine("Seed: Categories and products already exist, skipping.");
            return;
        }

        var now = DateTime.UtcNow;

        var categories = new List<Category>
        {
            new() { Name = "Laptops",       Description = "Portable personal computers",          IsActive = true, CreatedAt = now },
            new() { Name = "Smartphones",   Description = "Mobile phones and accessories",         IsActive = true, CreatedAt = now },
            new() { Name = "Peripherals",   Description = "Keyboards, mice, and input devices",    IsActive = true, CreatedAt = now },
            new() { Name = "Monitors",      Description = "Display screens and monitors",          IsActive = true, CreatedAt = now },
            new() { Name = "Networking",    Description = "Routers, switches, and network gear",   IsActive = true, CreatedAt = now },
            new() { Name = "Storage",       Description = "SSDs, HDDs, and flash storage",         IsActive = true, CreatedAt = now },
            new() { Name = "Audio",         Description = "Headphones, speakers, and microphones", IsActive = true, CreatedAt = now },
            new() { Name = "Components",    Description = "CPUs, GPUs, RAM, and motherboards",     IsActive = true, CreatedAt = now },
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        var laptops     = categories[0];
        var phones      = categories[1];
        var peripherals = categories[2];
        var monitors    = categories[3];
        var networking  = categories[4];
        var storage     = categories[5];
        var audio       = categories[6];
        var components  = categories[7];

        var products = new List<Product>
        {
            // Laptops
            new() { Name = "MacBook Pro 16\"",          Description = "Apple M3 Pro, 18GB RAM, 512GB SSD",               Price = 2499.99m, Stock = 15,  IsActive = true, CreatedAt = now, CategoryId = laptops.Id },
            new() { Name = "Dell XPS 15",               Description = "Intel Core i9, 32GB RAM, 1TB SSD, OLED display",  Price = 1999.99m, Stock = 20,  IsActive = true, CreatedAt = now, CategoryId = laptops.Id },
            new() { Name = "Lenovo ThinkPad X1 Carbon", Description = "Intel Core i7, 16GB RAM, 512GB SSD, lightweight", Price = 1599.99m, Stock = 25,  IsActive = true, CreatedAt = now, CategoryId = laptops.Id },
            new() { Name = "ASUS ROG Zephyrus G14",     Description = "AMD Ryzen 9, RTX 4060, 16GB RAM, 1TB SSD",        Price = 1449.99m, Stock = 18,  IsActive = true, CreatedAt = now, CategoryId = laptops.Id },
            new() { Name = "Microsoft Surface Laptop 5",Description = "Intel Core i5, 8GB RAM, 256GB SSD, touch screen", Price = 999.99m,  Stock = 30,  IsActive = true, CreatedAt = now, CategoryId = laptops.Id },
            new() { Name = "HP Spectre x360",           Description = "Intel Core i7, 16GB RAM, 512GB SSD, 2-in-1",      Price = 1349.99m, Stock = 22,  IsActive = true, CreatedAt = now, CategoryId = laptops.Id },

            // Smartphones
            new() { Name = "iPhone 15 Pro",             Description = "A17 Pro chip, 256GB, ProMotion display",           Price = 1099.99m, Stock = 50,  IsActive = true, CreatedAt = now, CategoryId = phones.Id },
            new() { Name = "Samsung Galaxy S24 Ultra",  Description = "Snapdragon 8 Gen 3, 512GB, S Pen included",        Price = 1299.99m, Stock = 40,  IsActive = true, CreatedAt = now, CategoryId = phones.Id },
            new() { Name = "Google Pixel 8 Pro",        Description = "Tensor G3, 256GB, AI-powered camera",              Price = 999.99m,  Stock = 35,  IsActive = true, CreatedAt = now, CategoryId = phones.Id },
            new() { Name = "OnePlus 12",                Description = "Snapdragon 8 Gen 3, 256GB, 100W fast charging",    Price = 799.99m,  Stock = 45,  IsActive = true, CreatedAt = now, CategoryId = phones.Id },

            // Peripherals
            new() { Name = "Logitech MX Keys S",        Description = "Wireless keyboard, backlit, multi-device",         Price = 119.99m,  Stock = 60,  IsActive = true, CreatedAt = now, CategoryId = peripherals.Id },
            new() { Name = "Logitech MX Master 3S",     Description = "Wireless mouse, 8K DPI, quiet clicks",             Price = 99.99m,   Stock = 75,  IsActive = true, CreatedAt = now, CategoryId = peripherals.Id },
            new() { Name = "Keychron Q1 Pro",           Description = "Mechanical keyboard, QMK/VIA, hot-swappable",      Price = 199.99m,  Stock = 40,  IsActive = true, CreatedAt = now, CategoryId = peripherals.Id },
            new() { Name = "Razer DeathAdder V3",       Description = "Gaming mouse, 30K DPI, ultra-lightweight",         Price = 69.99m,   Stock = 80,  IsActive = true, CreatedAt = now, CategoryId = peripherals.Id },
            new() { Name = "Stream Deck MK.2",          Description = "15 LCD keys, customizable macro pad",              Price = 149.99m,  Stock = 35,  IsActive = true, CreatedAt = now, CategoryId = peripherals.Id },

            // Monitors
            new() { Name = "LG UltraFine 5K",           Description = "27\", 5120x2880, Thunderbolt 3, P3 wide color",    Price = 1299.99m, Stock = 12,  IsActive = true, CreatedAt = now, CategoryId = monitors.Id },
            new() { Name = "Samsung Odyssey G7",        Description = "32\", 1440p, 240Hz, curved VA panel",              Price = 699.99m,  Stock = 20,  IsActive = true, CreatedAt = now, CategoryId = monitors.Id },
            new() { Name = "Dell UltraSharp U2723DE",   Description = "27\", 4K IPS, USB-C hub, color accurate",          Price = 749.99m,  Stock = 18,  IsActive = true, CreatedAt = now, CategoryId = monitors.Id },
            new() { Name = "ASUS ProArt PA32UCX",       Description = "32\", 4K Mini LED, HDR1400, professional grade",   Price = 1999.99m, Stock = 8,   IsActive = true, CreatedAt = now, CategoryId = monitors.Id },
            new() { Name = "BenQ PD2725U",              Description = "27\", 4K IPS, Thunderbolt 4, designer monitor",    Price = 799.99m,  Stock = 15,  IsActive = true, CreatedAt = now, CategoryId = monitors.Id },

            // Networking
            new() { Name = "Eero Pro 6E",               Description = "Tri-band mesh WiFi 6E, covers 2000 sq ft",         Price = 299.99m,  Stock = 30,  IsActive = true, CreatedAt = now, CategoryId = networking.Id },
            new() { Name = "Ubiquiti UniFi Dream Machine", Description = "All-in-one router, switch, and access point",   Price = 499.99m,  Stock = 15,  IsActive = true, CreatedAt = now, CategoryId = networking.Id },
            new() { Name = "TP-Link Deco XE75 Pro",     Description = "WiFi 6E mesh system, 2.5G ports, 3-pack",          Price = 449.99m,  Stock = 22,  IsActive = true, CreatedAt = now, CategoryId = networking.Id },
            new() { Name = "Netgear Nighthawk RS700S",  Description = "WiFi 7 router, 19Gbps, multi-link operation",      Price = 699.99m,  Stock = 10,  IsActive = true, CreatedAt = now, CategoryId = networking.Id },

            // Storage
            new() { Name = "Samsung 990 Pro 2TB",       Description = "NVMe PCIe 4.0 SSD, 7450MB/s read speed",          Price = 179.99m,  Stock = 50,  IsActive = true, CreatedAt = now, CategoryId = storage.Id },
            new() { Name = "WD Black SN850X 1TB",       Description = "NVMe PCIe 4.0 SSD, 7300MB/s, gaming optimized",   Price = 99.99m,   Stock = 60,  IsActive = true, CreatedAt = now, CategoryId = storage.Id },
            new() { Name = "Seagate IronWolf Pro 8TB",  Description = "NAS HDD, 7200 RPM, 300TB/year workload",           Price = 249.99m,  Stock = 25,  IsActive = true, CreatedAt = now, CategoryId = storage.Id },
            new() { Name = "Samsung T7 Shield 2TB",     Description = "Portable SSD, USB 3.2 Gen2, rugged, 1050MB/s",    Price = 159.99m,  Stock = 45,  IsActive = true, CreatedAt = now, CategoryId = storage.Id },
            new() { Name = "Crucial X9 Pro 4TB",        Description = "Portable SSD, USB-C, IP55 water resistant",        Price = 299.99m,  Stock = 30,  IsActive = true, CreatedAt = now, CategoryId = storage.Id },
            new() { Name = "SanDisk Extreme Pro 1TB",   Description = "Portable SSD, 2000MB/s, IP55, NVMe",              Price = 129.99m,  Stock = 55,  IsActive = true, CreatedAt = now, CategoryId = storage.Id },

            // Audio
            new() { Name = "Sony WH-1000XM5",           Description = "Over-ear ANC headphones, 30hr battery, LDAC",     Price = 349.99m,  Stock = 40,  IsActive = true, CreatedAt = now, CategoryId = audio.Id },
            new() { Name = "Apple AirPods Pro 2",       Description = "In-ear ANC, USB-C, Adaptive Audio, H2 chip",      Price = 249.99m,  Stock = 55,  IsActive = true, CreatedAt = now, CategoryId = audio.Id },
            new() { Name = "Bose QuietComfort Ultra",   Description = "Over-ear ANC, immersive audio, 24hr battery",     Price = 429.99m,  Stock = 30,  IsActive = true, CreatedAt = now, CategoryId = audio.Id },
            new() { Name = "Blue Yeti X",               Description = "USB condenser microphone, 4 polar patterns, LED", Price = 169.99m,  Stock = 35,  IsActive = true, CreatedAt = now, CategoryId = audio.Id },
            new() { Name = "Elgato Wave:3",             Description = "USB condenser mic, Clipguard, 96kHz/24-bit",      Price = 149.99m,  Stock = 40,  IsActive = true, CreatedAt = now, CategoryId = audio.Id },
            new() { Name = "Rode NT-USB Mini",          Description = "Compact USB microphone, studio quality, plug-n-play", Price = 99.99m, Stock = 50, IsActive = true, CreatedAt = now, CategoryId = audio.Id },
            new() { Name = "Sonos Era 300",             Description = "Wireless speaker, Dolby Atmos, spatial audio",    Price = 449.99m,  Stock = 20,  IsActive = true, CreatedAt = now, CategoryId = audio.Id },

            // Components
            new() { Name = "AMD Ryzen 9 7950X",         Description = "16-core desktop CPU, 5.7GHz boost, AM5 socket",   Price = 549.99m,  Stock = 20,  IsActive = true, CreatedAt = now, CategoryId = components.Id },
            new() { Name = "NVIDIA GeForce RTX 4080 Super", Description = "16GB GDDR6X, DLSS 3.5, Ada Lovelace arch",   Price = 999.99m,  Stock = 12,  IsActive = true, CreatedAt = now, CategoryId = components.Id },
            new() { Name = "Corsair Dominator Titanium 32GB", Description = "DDR5-6000, CL30, RGB, Intel XMP 3.0",      Price = 189.99m,  Stock = 35,  IsActive = true, CreatedAt = now, CategoryId = components.Id },
            new() { Name = "ASUS ROG Maximus Z790 Hero", Description = "Intel Z790, DDR5, PCIe 5.0, WiFi 6E",           Price = 599.99m,  Stock = 10,  IsActive = true, CreatedAt = now, CategoryId = components.Id },
            new() { Name = "Noctua NH-D15 G2",          Description = "Dual-tower CPU cooler, 140mm fans, LGA1851",      Price = 109.99m,  Stock = 28,  IsActive = true, CreatedAt = now, CategoryId = components.Id },
            new() { Name = "Corsair HX1000i",           Description = "1000W modular PSU, 80+ Platinum, ATX 3.0",        Price = 229.99m,  Stock = 22,  IsActive = true, CreatedAt = now, CategoryId = components.Id },
            new() { Name = "Lian Li PC-O11 Dynamic EVO", Description = "Mid-tower ATX case, dual chamber, tempered glass", Price = 179.99m, Stock = 18, IsActive = true, CreatedAt = now, CategoryId = components.Id },
            new() { Name = "Intel Core i9-14900K",      Description = "24-core desktop CPU, 6.0GHz boost, LGA1700",     Price = 529.99m,  Stock = 15,  IsActive = true, CreatedAt = now, CategoryId = components.Id },
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        Console.WriteLine($"Seed: {categories.Count} categories and {products.Count} products created.");
    }
}