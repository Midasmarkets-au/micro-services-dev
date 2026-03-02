-- Seed data for EventShopItemCategory Configuration
-- This file contains configuration entries for Event Shop Item Categories
-- Based on EventShopItemCategoryTypes enum values and ShopItemCategoryData structure

-- Clear existing EventShop configuration entries (optional - uncomment if needed)
-- DELETE FROM core."_Configuration" WHERE "Category" = 'Public' AND "Key" = 'EventShopItemCategoryKey';

-- Insert the EventShop Item Category configuration that the API expects
-- Category: 'Public', Key: 'EventShopItemCategoryKey'
-- Value: Dictionary<string, EventShopItem.ShopItemCategoryData>
INSERT INTO core."_Configuration" ("CreatedOn", "UpdatedOn", "UpdatedBy", "Name", "Value", "Category", "DataFormat", "Description", "Key", "RowId") VALUES 
(now(), now(), 0, 'EventShopItemCategoryKey', '{
  "BcrMerch": {
    "Status": true,
    "Value": 0,
    "Data": {
      "zh-cn": "BCR联名",
      "en-us": "BCR Merch"
    }
  },
  "Luxury": {
    "Status": true,
    "Value": 1,
    "Data": {
      "zh-cn": "奢侈品",
      "en-us": "Luxury"
    }
  },
  "Electronics": {
    "Status": true,
    "Value": 2,
    "Data": {
      "zh-cn": "电子产品",
      "en-us": "Electronics"
    }
  },
  "Vehicle": {
    "Status": true,
    "Value": 3,
    "Data": {
      "zh-cn": "汽车",
      "en-us": "Vehicle"
    }
  },
  "HomeAppliances": {
    "Status": true,
    "Value": 4,
    "Data": {
      "zh-cn": "小家电",
      "en-us": "Home Appliances"
    }
  },
  "LuxuryFood": {
    "Status": true,
    "Value": 5,
    "Data": {
      "zh-cn": "高端食品礼盒",
      "en-us": "Luxury Food"
    }
  },
  "Tobacco": {
    "Status": false,
    "Value": 6,
    "Data": {
      "zh-cn": "烟草",
      "en-us": "Tobacco"
    }
  },
  "Watch": {
    "Status": true,
    "Value": 7,
    "Data": {
      "zh-cn": "豪华手表",
      "en-us": "Watch"
    }
  },
  "Jewelry": {
    "Status": true,
    "Value": 8,
    "Data": {
      "zh-cn": "珠宝",
      "en-us": "Jewelry"
    }
  },
  "GiftCard": {
    "Status": true,
    "Value": 9,
    "Data": {
      "zh-cn": "礼品卡",
      "en-us": "Gift Card"
    }
  },
  "TravelPackage": {
    "Status": true,
    "Value": 10,
    "Data": {
      "zh-cn": "旅行套餐",
      "en-us": "Travel Package"
    }
  },
  "SkinCare": {
    "Status": true,
    "Value": 11,
    "Data": {
      "zh-cn": "高端护肤品",
      "en-us": "Skin Care"
    }
  },
  "CulterySet": {
    "Status": true,
    "Value": 12,
    "Data": {
      "zh-cn": "餐具套装",
      "en-us": "Cutlery Set"
    }
  },
  "Phone": {
    "Status": true,
    "Value": 13,
    "Data": {
      "zh-cn": "手机",
      "en-us": "Phone"
    }
  },
  "Furniture": {
    "Status": true,
    "Value": 14,
    "Data": {
      "zh-cn": "家具",
      "en-us": "Furniture"
    }
  },
  "Clock": {
    "Status": true,
    "Value": 15,
    "Data": {
      "zh-cn": "钟表",
      "en-us": "Clock"
    }
  },
  "Outdoor": {
    "Status": true,
    "Value": 16,
    "Data": {
      "zh-cn": "户外",
      "en-us": "Outdoor"
    }
  },
  "Bag": {
    "Status": true,
    "Value": 17,
    "Data": {
      "zh-cn": "包包",
      "en-us": "Bag"
    }
  },
  "Art": {
    "Status": true,
    "Value": 18,
    "Data": {
      "zh-cn": "艺术品",
      "en-us": "Art"
    }
  },
  "Makeup": {
    "Status": true,
    "Value": 19,
    "Data": {
      "zh-cn": "化妆品",
      "en-us": "Makeup"
    }
  },
  "LuxuryFurniture": {
    "Status": true,
    "Value": 20,
    "Data": {
      "zh-cn": "高端家具",
      "en-us": "Luxury Furniture"
    }
  },
  "H520": {
    "Status": true,
    "Value": 21,
    "Data": {
      "zh-cn": "七夕甄选",
      "en-us": "Chinese Valentine''s Day"
    }
  },
  "MidAutumnFestival": {
    "Status": true,
    "Value": 22,
    "Data": {
      "zh-cn": "中秋节日甄选",
      "en-us": "Mid-Autumn Festival"
    }
  },
  "ChineseBrandSkinCare": {
    "Status": true,
    "Value": 23,
    "Data": {
      "zh-cn": "国货护肤美妆",
      "en-us": "Chinese Brand Skin Care"
    }
  },
  "TravelTheWorld": {
    "Status": true,
    "Value": 24,
    "Data": {
      "zh-cn": "BCR-环游世界",
      "en-us": "BCR-Travel the World"
    }
  }
}', 'Public', 'json', 'Event Shop Item Categories with multi-language support', 'EventShopItemCategoryKey', 0);

-- Additional configuration for category settings
INSERT INTO core."_Configuration" ("CreatedOn", "UpdatedOn", "UpdatedBy", "Name", "Value", "Category", "DataFormat", "Description", "Key", "RowId") VALUES 
(now(), now(), 0, 'EventShopItemCategorySettings', '{
  "defaultLanguage": "zh_cn",
  "supportedLanguages": ["zh_cn", "en_us"],
  "excludeFromAvailable": [6],
  "featuredCategories": [0, 1, 2, 24],
  "displayOrder": [24, 23, 0, 1, 2, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22]
}', 'EventShop', 'json', 'Event Shop Item Category Settings and Configuration', 'EventShopItemCategorySettings', 0); 
