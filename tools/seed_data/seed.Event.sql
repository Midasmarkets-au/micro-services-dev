INSERT INTO event."_Event"(
    "ApplyStartOn", "ApplyEndOn", "StartOn", "EndOn", "AccessRoles", "Key", "Status", "CreatedOn", "UpdatedOn", "AccessSites")
    VALUES (now(), '2026-06-19 04:01:33.185665+00', now(), '2026-06-19 04:01:33.185665+00', '["Guest","Admin", "TenantAdmin","Client"]', 'EventShop', 1, now(), now(), '[]');

-- Insert event party relationships for existing users
INSERT INTO event."_EventParty"(
    "EventId", "PartyId", "Status", "Settings", "CreatedOn", "UpdatedOn", "OperatorPartyId")
    VALUES 
    (
        (SELECT "Id" FROM event."_Event" WHERE "Key" = 'EventShop'),
        10001, -- Admin User
        1, -- Active
        '{"role": "admin", "permissions": ["view_all", "manage_orders"]}',
        now(),
        now(),
        1
    );
    
-- Insert event language data (English)
INSERT INTO event."_EventLanguage"(
    "EventId", "Language", "Name", "Title", "Description", "images", "Term", "CreatedOn", "UpdatedOn", "Instruction")
    VALUES (
        (SELECT "Id" FROM event."_Event" WHERE "Key" = 'EventShop'),
        'en-us',
        'EventShop 2025',
        'Premium Rewards EventShop',
        'Earn points and redeem exclusive rewards from our premium collection including luxury items, electronics, and travel packages.',
        '{
  "banner": "https://midasmarkets.s3.amazonaws.com/t_1/25/06/5d1ada9c-f257-4a80-869f-e0646d102221_86f82f6b.jpg",
  "hero": "https://midasmarkets.s3.amazonaws.com/t_1/25/06/5d1ada9c-f257-4a80-869f-e0646d102221_86f82f6b.jpg",
  "thumbnail": "https://midasmarkets.s3.amazonaws.com/t_1/25/06/5d1ada9c-f257-4a80-869f-e0646d102221_86f82f6b.jpg"
}',$Term$<p>Introduction</p><p>BCR Points Mall (hereinafter referred to as \"Points Mall\") is operated by BCR Co Pty Ltd (Company Number 1975046, trading as BCR, hereinafter referred to as \"the Company\", \"we\", or \"BCR\").</p><p>Eligibility and Conditions</p><p>2.1 BCR Points Mall is open to both new and existing customers who are at least 18 years old and hold a valid trading account that meets BCR's account opening requirements.</p><p>2.2 Points Mall is only applicable to real account types provided by BCR (Standard, Preferred, and Alpha accounts are eligible, excluding institutional accounts).</p><p>2.3 You agree to receive marketing communications from BCR even if you have opted out previously.</p><p>2.4 BCR reserves the right to suspend or revoke the Points Mall at any time, and eligibility of accounts is at our discretion.</p><p>BCR's Rights</p><p>3.1 BCR reserves the right to refuse any participation in promotional activities or instructions without providing a reason or explanation.</p><p>3.2 BCR reserves the right to exclude and/or cancel a customer's participation in any promotions or Points Mall offered under the following circumstances, but not limited to:</p><p>a) The customer conducts abusive trading practices; or</p><p>b) The customer is found to have violated BCR's customer agreement or the terms and conditions of the Points Mall. In such cases, BCR reserves the right to deduct any points from the customer's Points Mall.</p><p>3.3 If BCR has reasonable grounds to believe that you have abused the Points Mall (including improper, unreasonable, fraudulent, or illegal use, or you have breached any obligations under BCR policy), we have the right to immediately terminate or suspend your eligibility.</p><p>3.4 If the account holder passes away, family members or cohabitants can contact us within 12 months to claim any remaining points.</p><p>3.5 BCR reserves the right to decide at its discretion whether a trading account is eligible to accumulate points in the Points Mall. We may restrict the eligibility of certain trading accounts, such as those participating in other specific BCR promotional activities.</p><p>Points Use and Redemption Terms</p><p>4.1 Points are non-transferrable and can only be redeemed by the account holder.</p><p>4.2 Points in the BCR Points Mall expire on December 31, 2026; points not redeemed by this date will become void.</p><p>4.3 Points are earned for every standard lot traded on forex, commodities, and certain indices in your BCR account. The following products are excluded from earning points: #AUS200, #GER40, #EUSTX50, #ESP35, #FRA40, #UK100, and all stocks.</p><p>4.4 Trading volume is calculated based on closed orders, and points are immediately displayed in your Points Mall account.</p><p>Product Adjustments</p><p>5.1 You may use account points to redeem available products on the Points Mall. After completing an order, the corresponding points will be deducted from your account.</p><p>5.2 Normally, we will arrange product shipment within 5 working days after order confirmation. We are not responsible for any losses caused by product out of stock, discontinued supply, shipping delays, or unauthorized fraudulent redemptions.</p><p>5.3 If we are unable to deliver products to the customer's address due to force majeure (e.g., product shortages, logistic issues, etc.), you may cancel the order, and the corresponding points will be refunded to your account.</p><p>5.4 The availability of listed products depends on actual conditions. We cannot guarantee that all products will be available at the time of redemption. If we are unable to deliver according to the promotional activity or if products are no longer available for purchase, you agree to accept that the corresponding points will be refunded to your account.</p><p>5.5 Products offered in the BCR Points Mall are limited to the product itself and do not include taxes or other additional fees.</p><p>Change or Removal of Products</p><p>6.1 BCR reserves the right to change or remove products available for redemption in the Points Mall at its discretion. Any changes can be checked by logging into the BCR Points Mall portal.</p><p>Limitation of Liability</p><p>7.1 BCR is not liable for any direct or indirect losses (including but not limited to indirect losses, special losses, or loss of profits), expenses, expenditures, or damages suffered due to the use of the Points Mall.</p><p>Final Interpretation</p><p>8.1 Any disputes or situations not covered by these terms will be resolved at the discretion of BCR.</p><p>Your Information</p><p>9.1 You agree to the collection, use, and disclosure of your information in accordance with these terms and our privacy policy listed on thebcr.com website.</p><p>Changes to Terms and Conditions</p><p>10.1 We may need to change these terms and conditions from time to time. Changes will be posted online.</p><p>Applicable Law and Jurisdiction</p><p>11.1 Our services are not targeted at residents of the USA and Canada, nor do we intend to distribute or use the information provided in any country or jurisdiction where such distribution or use would be contrary to local law or regulation.</p><p>Participants in the Points Mall should confirm they have read and agree to abide by these terms and conditions, as well as BCR's customer agreement.</p><p><br></p><p>Disclaimer</p><p>This document is provided by BCR Co Pty Ltd (Company Number 1975046). Please note that trading in leveraged off-exchange financial products carries significant risks and may result in significant losses. Your losses may exceed your initial deposits, and therefore, financial products may not be suitable for all investors. Before deciding to participate in this reward activity, you should refer to the product guide available on our website www.thebcr.com to understand the risks involved in off-exchange forex and derivatives trading. BCR shall not be liable for any losses, costs, expenses, or damages incurred due to your participation in this reward activity, and such losses, costs, expenses, or damages shall not be excluded from these terms according to the law. Derivative trading may not be suitable for everyone, so ensure you fully understand the risks involved. BCR does not issue any personal financial advice, recommendations, or opinions related to acquiring, holding, or disposing of derivatives. BCR is not a financial advisor; our services are provided on an execution basis only. The information involved in this promotion is not intended to be distributed to, or used by, any person in any country or jurisdiction where such distribution or use would contravene local law or regulation. BCR reserves the right to amend these terms and conditions at any time without notice.</p>$Term$,
        now(),
        now(),
        $JSON${
          "pointsRule": {
              "all": "<p><br></p>",
              "agent": "<p><strong style=\"color: var(--tw-prose-bold);\">Agent Points:</strong></p><ul><li>Customer Trading Points: For each lot traded by clients under the agent, 0.3 points will be added.</li></ul><p><strong style=\"color: var(--tw-prose-bold);\">Notes:</strong></p><ul><li>Agents only receive points from the trades of their direct clients.</li><li>Trades closed within one minute do not generate points.</li></ul><p><br></p><p><strong style=\"color: var(--tw-prose-bold);\">Notes:</strong></p><ul><li>Cashback rewards are automatically credited to the agent's wallet after the client closes the trade. Trades closed within one minute are not included.</li></ul><p><br></p>",
              "sales": "<p><strong style=\"color: var(--tw-prose-bold);\">Agent Points:</strong></p><ul><li>Customer Trading Points: For each lot traded by clients under the agent, 0.3 points will be added.</li></ul><p><strong style=\"color: var(--tw-prose-bold);\">Notes:</strong></p><ul><li>Agents only receive points from the trades of their direct clients.</li><li>Trades closed within one minute do not generate points.</li></ul><p><br></p><p><strong style=\"color: var(--tw-prose-bold);\">Notes:</strong></p><ul><li>Cashback rewards are automatically credited to the agent's wallet after the client closes the trade. Trades closed within one minute are not included.</li></ul><p><br></p><p><strong style=\"color: var(--tw-prose-bold);\">Sales Points Acquisition Rules:</strong></p><ul><li>New Agent Points:</li></ul><ol><li>For each new valid agent account, 5 points will be awarded.</li></ol><ul><li>New Customer Points:</li></ul><ol><li>For each new valid customer under the agent, 1 point will be awarded.</li><li>For each new DC customer, 6 points will be awarded.</li></ol><p><strong style=\"color: var(--tw-prose-bold);\">Notes:</strong></p><ul><li>A valid agent account must have at least one valid customer who is not under the same name.</li><li>A valid customer must have at least one deposit.</li></ul><p><br></p><p><br></p><p><br></p>",
              "client": "<p><strong style=\"color: var(--tw-prose-bold);\">Customer Trading Points:</strong></p><ul><li><span style=\"color: var(--tw-prose-bold);\">Closing Trade Points: Users earn 1 point for each closed trade lot. Trades closed in less than one minute do not generate points.</span></li></ul><p><br></p><p><strong style=\"color: var(--tw-prose-bold);\">Notes:</strong></p><ul><li>All cashback rewards are automatically credited to the trading account after closing the trade.</li><li>Trades held for less than one minute are not eligible for rewards.</li><li>Points earned through EA or other copying programs and credit card deposit trading accounts will not be counted.</li></ul><p><br></p>"
          }
        }$JSON$::jsonb
    );
-- Insert event language data (Chinese)
INSERT INTO event."_EventLanguage"(
    "EventId", "Language", "Name", "Title", "Description", "images", "Term", "CreatedOn", "UpdatedOn", "Instruction")
    VALUES (
        (SELECT "Id" FROM event."_Event" WHERE "Key" = 'EventShop'),
        'zh-cn',
        '活动商城 2025',
        '精品奖励活动商城',
        '赚取积分并兑换我们精品收藏中的独家奖励，包括奢侈品、电子产品和旅行套餐。',
        '{
  "banner": "https://midasmarkets.s3.amazonaws.com/t_1/25/06/5d1ada9c-f257-4a80-869f-e0646d102221_86f82f6b.jpg",
  "hero": "https://midasmarkets.s3.amazonaws.com/t_1/25/06/5d1ada9c-f257-4a80-869f-e0646d102221_86f82f6b.jpg",
  "thumbnail": "https://midasmarkets.s3.amazonaws.com/t_1/25/06/5d1ada9c-f257-4a80-869f-e0646d102221_86f82f6b.jpg"
}',$Term$,<p>BCR积分商城条款和条件</p><p>1：介绍 </p><p>BCR积分商城（以下简称“积分商城”）是由BCR Co Pty Ltd（公司编号1975046，交易名为BCR，以下简称“公司”、“我们”或“BCR”）运营的。</p><p><br></p><p>2. 参与资格及条件 </p><p>2.1 BCR积分商城对于年满18周岁及以上，且持有符合BCR开户要求的有效交易账户的新老客户开放。</p><p>&nbsp;2.2 积分商城仅适用于由BCR提供的实盘账户类型（标准账户，优选账户，阿尔法账户均可参加，机构账户除外）。</p><p>2.3 您同意接收BCR的营销通讯，即使您之前选择了不接收。 </p><p>2.4 BCR保留随时暂停或撤销积分商城的权利，并且账户的合格性将受到我们的决定权的约束。</p><p><br></p><p>3. BCR 的权利&nbsp;</p><p>3.1 BCR有权自行决定拒绝任何参与促销活动的申请或指示，无需提供理由或解释。</p><p>3.2 BCR保留在以下情况下（但不限于此），排除和/或取消客户参与其提供的优惠或积分商城的权利： </p><p>a) 客户恶意进行违规交易；或 </p><p>b) 客户被发现违反了BCR的客户协议或积分商城的条款和条件。在这种情况下，BCR保留从客户的积分商城中扣除任何积分权利。 </p><p>3.3 如果BCR有合理理由相信您滥用积分商城（包括您的使用方式不当、不合理、欺诈或非法，或您违反了BCR政策下的义务），我们有权立即终止或暂停您的资格。</p><p>3.4如果账户持有人去世，家庭成员或与其共同居住的人可在12个月内与我们联系并索取剩余积分。 </p><p>3.5 BCR保留自行决定交易账户是否有资格在积分商城中积累积分的权利。我们可能会限制某些交易账户的资格，例如那些参与其他特定BCR促销活动的账户。</p><p><br></p><p>4. 积分使用和兑换条款 </p><p>4.1 积分仅限账户持有人兑换，不可转让。 </p><p>4.2 BCR积分商城的积分效期截止时间为2026年12月31日，逾期作废。 </p><p>4.3 您在BCR账户上进行的每标准手外汇、大宗商品以及部分股指交易将获得1点积分。以下产品不参与积分获取：#AUS200, #GER40, #EUSTX50, #ESP35, #FRA40, #UK100以及所有股票。 </p><p>4.4 交易量将根据已平仓订单进行计算，并且积分将立即显示在您的积分商城账户上。</p><p><br></p><p>5. 产品调整 </p><p>5.1 您可以使用账户积分在积分商城上兑换可用的产品。完成订单后，相应积分将从您的账户中扣除。 </p><p>5.2 通常情况下，我们将在订单确认后的5个工作日内安排产品发货。对于因产品售罄、停止供应、发货延迟或未经授权人员进行欺诈性兑换而造成的任何损失，我们概不负责。 </p><p>5.3 如果由于不可抗力因素（例如产品缺货、物流问题等）导致我们无法将产品送达客户地址，您可以取消订单，相应积分将退还至您的账户。 </p><p>5.4 所列出的产品供应情况取决于实际情况。我们无法保证所有产品在兑换时都有供应。如果我们无法按照促销活动发货或产品已不再可购买，您同意接受相应积分将退还至您的账户。 </p><p>5.5 BCR积分商城提供的产品仅限于产品本身，不包括税务或其他额外费用。</p><p><br></p><p>6. 更改或删除产品 </p><p>6.1 BCR保留自行决定更改或删除可在积分商城中兑换的产品的权利。若有任何更改，您可通过登录BCR积分商城门户网站查询。</p><p><br></p><p>7. 责任限制 </p><p>7.1 BCR对因使用积分商城而遭受的任何直接或间接损失（包括但不限于间接损失、特殊损失或利润损失）、费用、支出或损害不承担责任。 </p><p><br></p><p>8. 最终解释权 </p><p>8.1 本条款未涵盖的任何争议或情况将由BCR自行决定解决。 </p><p><br></p><p>9. 您的信息 </p><p>9.1 您同意按照本条款和我们在thebcr.com网站上列出的隐私政策收集、使用和披露您的信息。</p><p><br></p><p>10. 更改条款和条件 </p><p>10.1 我们可能需要随时更改这些条款和条件。我们将在网上发布这些条款。 </p><p><br></p><p>11. 适用的法律和司法管辖权 </p><p>11.1 我们的服务并不针对美国和加拿大的居民，我们也不打算在任何与当地法律或法规相抵触的国家或地区分发或使用所提供的信息。</p><p><br></p><p>参与积分商城的客户应确认已阅读并同意遵守这些条款和条件，以及BCR的客户协议。 </p><p><br></p><p>免责条款 </p><p>该文件由 BCR Co Pty Ltd(公司编号1975046) 提供。请注意，场外杠杆金融产品交易存在重大风险，并且有可能导致重大损失。您的损失可能超过您的初始存入金额，因此，理财产品可能不适合所有投资者。在决定是否参加此奖励活动之前，您应当参考我们网站www.thebcr.com上提供的产品指南，了解场外外汇和衍生品交易所涉及的相关风险。因您参与本奖励活动而可能遭受的任何损失、成本、费用或损害，BCR概不担责，并且，根据法律规定，该等损失、成本、费用或损害不得排除在该等条款之外。</p><p>衍生品交易可能并不适合所有人，因此，请确保您已经充分了解了相关风险。BCR不发布任何与获取、持有或处置衍生品相关的个人财务建议、推荐或意见。BCR并非财务顾问，本公司仅在执行的基础上提供各项服务。本推广活动中涉及的信息并非有意分发至或者由任何国家或司法管辖区的任何人员使用，因为，此类分发或使用可能违反当地法律或法规。BCR保留权利，有权随时修改本条款和细则，恕不另行通知。</p>$Term$,
        now(),
        now(),
        $JSON${
          "pointsRule": {
            "all": "<p><br></p>",
            "agent": "<h4>代理积分：</h4><ul><li>客户交易积分：代理名下客人每交易1手，增加0.3分积分。</li></ul><h4>注意事项：</h4><ul><li>代理只享受直属客户交易所产生的积分。</li><li>一分钟内的平仓单不产生积分。</li></ul><h4><br></h4><h4>注意事项：</h4><ul><li>客户平仓后回馈奖励自动打入代理账户钱包，<span style=\"color: rgb(94, 98, 120);\">一分钟内的平仓单</span>不计入。</li></ul>",
            "sales": "<h4>代理积分：</h4><ul><li>客户交易积分：代理名下客人每交易1手，增加0.3分积分。</li></ul><h4>注意事项：</h4><ul><li>代理只享受直属客户交易所产生的积分。</li><li>一分钟内的平仓单不产生积分。</li></ul><h4><br></h4><h4>注意事项：</h4><ul><li>客户平仓后回馈奖励自动打入代理账户钱包，一分钟内的平仓单不计入。</li></ul><p><br></p><h4>销售积分获取规则：</h4><ul><li>新开代理积分：</li></ul><ol><li>每新开一个有效代理账号，获得5积分。</li></ol><ul><li>新开客户积分：</li></ul><ol><li>代理名下的新开有效客户，获得1积分。</li><li>新DC客户，获得6积分。</li></ol><p><strong>注意事项：</strong></p><ul><li>有效代理账号至少需有一个非同名的有效客户。</li><li>有效客户需至少有一笔入金。</li></ul><p><br></p><h4><br></h4>",
            "client": "<h4>客户交易积分：</h4><ul><li><span style=\"color: var(--tw-prose-bold);\">平仓交易积分</span>：用户每平仓1手交易，获得1积分。如果持仓时间少于一分钟的平仓订单，将不产生积分。</li></ul><h4><br></h4><h4>注意事项：</h4><ul><li>所有回馈将在平仓后自动打入该交易账户。</li><li>持仓时间少于一分钟的订单不参与奖励。</li><li>通过EA或其他跟单程序交易及信用卡存款交易帐户之积分将不作计算。</li></ul><p><br></p>"
          }
        }$JSON$::jsonb
    );

-- Insert event shop points for participants 可能有问题, 用Tenant Admin UI来添加比较安全
INSERT INTO event."_EventShopPoint"(
    "EventPartyId", "Point", "TotalPoint", "FrozenPoint", "CreatedOn", "UpdatedOn")
    VALUES 
    (
        (SELECT "Id" FROM event."_EventParty" WHERE "PartyId" = 10001 AND "EventId" = (SELECT "Id" FROM event."_Event" WHERE "Key" = 'EventShop')),
        50000, -- Current points
        50000, -- Total earned
        0,     -- Frozen points
        now(),
        now()
    ),
    (
        (SELECT "Id" FROM event."_EventParty" WHERE "PartyId" = 10002 AND "EventId" = (SELECT "Id" FROM event."_Event" WHERE "Key" = 'EventShop')),
        15000, -- Current points
        25000, -- Total earned (some already spent)
        2000,  -- Frozen points
        now(),
        now()
    ),
    (
        (SELECT "Id" FROM event."_EventParty" WHERE "PartyId" = 10003 AND "EventId" = (SELECT "Id" FROM event."_Event" WHERE "Key" = 'EventShop')),
        8500,  -- Current points
        12000, -- Total earned
        500,   -- Frozen points
        now(),
        now()
    );

-- Insert luxury watch
INSERT INTO event."_EventShopItem"(
    "EventId", "Type", "Category", "AccessRoles", "Configuration", "Point", "Status", "CreatedOn", "UpdatedOn")
    VALUES (
        (SELECT "Id" FROM event."_Event" WHERE "Key" = 'EventShop'),
        0, -- Product
        7, -- Watch
        '["Client", "Admin"]',
        '{"brand": "Rolex", "model": "Submariner", "warranty": "2 years", "shipping": {"weight": 0.5, "dimensions": "15x15x10"}}',
        45000,
        1, -- Active
        now(),
        now()
    );

-- Insert electronics item
INSERT INTO event."_EventShopItem"(
    "EventId", "Type", "Category", "AccessRoles", "Configuration", "Point", "Status", "CreatedOn", "UpdatedOn")
    VALUES (
        (SELECT "Id" FROM event."_Event" WHERE "Key" = 'EventShop'),
        0, -- Product
        2, -- Electronics
        '["Client", "Admin"]',
        '{"brand": "Apple", "model": "iPhone 15 Pro", "color": "Natural Titanium", "storage": "256GB", "warranty": "1 year"}',
        25000,
        1, -- Active
        now(),
        now()
    );

-- Insert travel package
INSERT INTO event."_EventShopItem"(
    "EventId", "Type", "Category", "AccessRoles", "Configuration", "Point", "Status", "CreatedOn", "UpdatedOn")
    VALUES (
        (SELECT "Id" FROM event."_Event" WHERE "Key" = 'EventShop'),
        0, -- Product
        10, -- Travel Package
        '["Client", "Admin"]',
        '{"destination": "Maldives", "duration": "7 days", "accommodation": "5-star resort", "includes": ["flights", "meals", "activities"]}',
        80000,
        1, -- Active
        now(),
        now()
    );

-- Insert gift card
INSERT INTO event."_EventShopItem"(
    "EventId", "Type", "Category", "AccessRoles", "Configuration", "Point", "Status", "CreatedOn", "UpdatedOn")
    VALUES (
        (SELECT "Id" FROM event."_Event" WHERE "Key" = 'EventShop'),
        0, -- Product
        9, -- Gift Card
        '["Client", "Admin"]',
        '{"provider": "Amazon", "value": 500, "currency": "USD", "expiry": "2 years"}',
        12000,
        1, -- Active
        now(),
        now()
    );

-- Luxury Watch - English
INSERT INTO event."_EventShopItemLanguage"(
    "EventShopItemId", "Language", "Name", "Title", "Description", "Images", "CreatedOn", "UpdatedOn")
    VALUES (
        (SELECT "Id" FROM event."_EventShopItem" WHERE "Configuration"->>'brand' = 'Rolex'),
        'en-us',
        'Rolex Submariner',
        'Luxury Swiss Watch - Rolex Submariner',
        'The Rolex Submariner is a legendary diving watch, perfect for both underwater adventures and elegant occasions. Features automatic movement, 300m water resistance, and iconic design.',
        '[{"url": "https://example.com/rolex1.jpg", "alt": "Rolex Submariner Front"}, {"url": "https://example.com/rolex2.jpg", "alt": "Rolex Submariner Side"}]',
        now(),
        now()
    );

-- Luxury Watch - Chinese
INSERT INTO event."_EventShopItemLanguage"(
    "EventShopItemId", "Language", "Name", "Title", "Description", "Images", "CreatedOn", "UpdatedOn")
    VALUES (
        (SELECT "Id" FROM event."_EventShopItem" WHERE "Configuration"->>'brand' = 'Rolex'),
        'zh-cn',
        '劳力士潜航者',
        '奢华瑞士手表 - 劳力士潜航者',
        '劳力士潜航者是传奇的潜水手表，适合水下冒险和优雅场合。具有自动机芯、300米防水和标志性设计。',
        '[{"url": "https://example.com/rolex1.jpg", "alt": "劳力士潜航者正面"}, {"url": "https://example.com/rolex2.jpg", "alt": "劳力士潜航者侧面"}]',
        now(),
        now()
    );

-- iPhone - English
INSERT INTO event."_EventShopItemLanguage"(
    "EventShopItemId", "Language", "Name", "Title", "Description", "Images", "CreatedOn", "UpdatedOn")
    VALUES (
        (SELECT "Id" FROM event."_EventShopItem" WHERE "Configuration"->>'brand' = 'Apple'),
        'en-us',
        'iPhone 15 Pro',
        'Apple iPhone 15 Pro - 256GB Natural Titanium',
        'The most advanced iPhone ever with titanium design, A17 Pro chip, and professional camera system. Includes 256GB storage and comes with full warranty.',
        '[{"url": "https://example.com/iphone1.jpg", "alt": "iPhone 15 Pro Front"}, {"url": "https://example.com/iphone2.jpg", "alt": "iPhone 15 Pro Back"}]',
        now(),
        now()
    );

-- Travel Package - English
INSERT INTO event."_EventShopItemLanguage"(
    "EventShopItemId", "Language", "Name", "Title", "Description", "Images", "CreatedOn", "UpdatedOn")
    VALUES (
        (SELECT "Id" FROM event."_EventShopItem" WHERE "Configuration"->>'destination' = 'Maldives'),
        'en-us',
        'Maldives Luxury Getaway',
        '7-Day Maldives Paradise - 5-Star Resort Experience',
        'Experience paradise with this exclusive 7-day Maldives package. Includes round-trip flights, 5-star resort accommodation, all meals, and water activities. Perfect for couples or solo travelers.',
        '[{"url": "https://example.com/maldives1.jpg", "alt": "Maldives Resort"}, {"url": "https://example.com/maldives2.jpg", "alt": "Maldives Beach"}]',
        now(),
        now()
    );

-- Gift Card - English
INSERT INTO event."_EventShopItemLanguage"(
    "EventShopItemId", "Language", "Name", "Title", "Description", "Images", "CreatedOn", "UpdatedOn")
    VALUES (
        (SELECT "Id" FROM event."_EventShopItem" WHERE "Configuration"->>'provider' = 'Amazon'),
        'en-us',
        'Amazon Gift Card',
        '$500 Amazon Gift Card - Digital Delivery',
        'Versatile $500 Amazon gift card perfect for shopping electronics, books, home goods, and more. Digital delivery within 24 hours. Valid for 2 years from purchase date.',
        '[{"url": "https://example.com/amazon-card.jpg", "alt": "Amazon Gift Card"}]',
        now(),
        now()
    );

-- Insert sample addresses for order fulfillment
INSERT INTO core."_Address"(
    "PartyId", "Name", "CCC", "Phone", "Country", "Content", "CreatedOn", "UpdatedOn")
    VALUES 
    (
        10002, -- John Smith
        'John Smith',
        '+1',
        '555-0123',
        'United States',
        '123 Main Street, Apt 4B, New York, NY 10001',
        now(),
        now()
    );

-- Insert sample orders
INSERT INTO event."_EventShopOrder"(
    "EventPartyId", "EventShopItemId", "Quantity", "TotalPoint", "AddressId", "Status", "Comment", "Shipping", "CreatedOn", "UpdatedOn", "OperatorPartyId")
    VALUES 
    (
        (SELECT "Id" FROM event."_EventParty" WHERE "PartyId" = 10001 AND "EventId" = (SELECT "Id" FROM event."_Event" WHERE "Key" = 'EventShop')),
        (SELECT "Id" FROM event."_EventShopItem" WHERE "Configuration"->>'provider' = 'Amazon'),
        1,
        12000,
        (SELECT "Id" FROM core."_Address" WHERE "PartyId" = 10002 LIMIT 1),
        2, -- Completed
        'Digital delivery completed successfully',
        '{"method": "digital", "tracking": "AMZN-12345", "delivered_on": "2025-01-15T10:30:00Z"}',
        now() - interval '5 days',
        now() - interval '3 days',
        10001
    );