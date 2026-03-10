<template>
  <div id="new-kyc-form" style="position: relative">
    <div style="position: absolute; width: 100%; height: 100%"></div>
    <div class="d-flex flex-flow justify-content-between">
      <div>
        <img width="40" height="40" src="/images/logos/logo.png" />
      </div>
      <div class="fs-11">
        <!-- <div class="text-end">xx-xxxx-xxxxxxxx</div> -->
        <div class="text-end">Licensed Financial Institution</div>
        <div class="d-flex text-end mt-1">
          <div>Account 账号 #:</div>
          <div class="ms-2">
            <span style="text-decoration: underline">
              &nbsp;&nbsp;{{ props.kycInfos.accountNumber }}&nbsp;&nbsp;
            </span>
          </div>
        </div>
      </div>
    </div>

    <div class="text-center mt-11 mb-11">
      <div class="fs-1">
        KYC EVIDENCE VERIFICATION FORM / 客户信息核实证明表
      </div>
      <div class="fs-1">MM Co Ltd</div>
      <div class="fs-3">
        For Individuals, Individual Beneficial Owners and Sole Traders
      </div>
      <div class="fs-3">供个人，个人实益拥有者和个体经营者使用</div>
    </div>

    <div>
      <div class="mb-5">
        I. Collection of Minimum KYC Information 客户基础信息采集
      </div>

      <table style="width: 100%">
        <tr>
          <td class="kyc-label-col-2">
            <span>Surname</span>
            <br />
            <span>姓</span>
          </td>
          <td class="kyc-input-col-3">{{ props.kycInfos.lastName }}</td>
          <td class="kyc-label-col-2">
            <span>Given Name:</span>
            <br />
            <span>名</span>
          </td>
          <td class="kyc-input-col-3">{{ props.kycInfos.firstName }}</td>
        </tr>

        <tr>
          <td class="kyc-label-col-2">
            <span>Prior Name</span>
            <br />
            <span>曾用名</span>
          </td>
          <td class="kyc-input-col-3">{{ props.kycInfos.priorName }}</td>
          <td class="kyc-label-col-2">
            <span>Date of Birth</span>
            <br />
            <span>出生日期</span>
          </td>
          <td class="kyc-input-col-3">{{ props.kycInfos.birthday }}</td>
        </tr>

        <tr>
          <td class="kyc-label-col-2">
            <span>Residential Address</span>
            <br />
            <span>居住地址</span>
          </td>
          <td colspan="3" class="kyc-input-col-8">
            {{ props.kycInfos.address }}
          </td>
        </tr>

        <tr>
          <td class="kyc-label-col-2">
            <span>Social Media Accounts</span>
            <br />
            <span>社交媒体帐户 </span>
            <br />
            <br />
            <span> [ WhatsApp/WeChat ( 微信 ) /Instagram/Telegram ]</span>
          </td>
          <td colspan="3" class="kyc-input-col-8 fs-3">
            <div
              v-for="(type, index) in kycInfos.socialMediaTypes"
              :key="index"
            >
              <div class="d-flex" v-if="type.account != ''">
                <div class="me-3">{{ $t("fields." + type.name) }}:</div>
                <div>
                  {{ type.account }}
                </div>
              </div>
            </div>
          </td>
        </tr>

        <tr>
          <td class="kyc-label-col-2">
            <span>AU Citizen or hold a passport from another country?</span>
            <br />
            <span>是否为澳大利亚联邦公民或持有他国护照?</span>
          </td>
          <td colspan="2" class="kyc-input-col-3">
            <div style="font-size: 12px">
              <div class="d-flex align-items-center mb-3">
                <input
                  class="me-4"
                  type="checkbox"
                  :checked="props.kycInfos.citizen == 'au'"
                />
                <div>AU Citizen 澳大利亚联邦公民</div>
              </div>
              <div class="d-flex">
                <div>
                  <div class="d-flex align-items-center">
                    <input
                      class="me-4"
                      type="checkbox"
                      :checked="props.kycInfos.citizen != 'au'"
                    />
                    <div>Foreign Passport Holder 外国护照持有人</div>
                  </div>
                </div>
              </div>
            </div>
          </td>

          <td colspan="2" class="kyc-input-col-3">
            <div style="font-size: 12px">
              Please provide details <br />请提供细节:
              <strong class="ms-2" style="text-transform: uppercase"
                >{{ props.kycInfos.citizen }} Citizen</strong
              >
            </div>
          </td>
        </tr>

        <tr>
          <td class="kyc-label-col-2">
            <span>E-mail Address</span>
            <br />
            <span>电子邮箱</span>
          </td>
          <td colspan="3" class="kyc-input-col-8">
            {{ props.kycInfos.email }}
          </td>
        </tr>

        <tr>
          <td class="kyc-label-col-2">
            <span>Industry of Employment</span>
            <br />
            <span>工作行业</span>
          </td>
          <td class="kyc-input-col-3">
            {{ props.kycInfos.industry }}
          </td>
          <td class="kyc-label-col-2">
            <span>Position</span>
            <br />
            <span>职位</span>
          </td>
          <td class="kyc-input-col-3">
            {{ $t("fields." + props.kycInfos.occupation) }}
          </td>
        </tr>

        <tr>
          <td class="kyc-label-col-2">
            <span>Annual Income</span>
            <br />
            <span>年收入</span>
          </td>

          <td class="kyc-input-col-3">
            {{ $t("fields.range_" + props.kycInfos.annualIncome) }}
          </td>
          <td rowspan="2" class="kyc-label-col-2">
            <span>How might you fund your trading:</span>
            <br />
            <span>投资资金来源</span>
          </td>
          <td rowspan="2" class="kyc-input-col-3">
            {{ props.kycInfos.sourceOfFunds }}
          </td>
        </tr>
        <tr>
          <td class="kyc-label-col-2">
            <span>Savings and Investments</span>
            <br />
            <span>储蓄与投资价值</span>
          </td>
          <td class="kyc-input-col-3">
            {{ $t("fields.range_" + props.kycInfos.netWorth) }}
          </td>
        </tr>

        <tr>
          <td colspan="2" class="kyc-label-col-2">
            <span
              >Have you ever attended an education seminar or course on trading
              Stocks, Bonds, Contracts-for-difference, Forex, Derivatives, or
              other securities trading?</span
            >
            <br />
            <span
              >您是否参加过有关股票、债券、差价合约、外汇、衍生品或其他证券交易交易的教育研讨会或课程？</span
            >
          </td>

          <td colspan="3" class="kyc-input-col-8" style="font-size: 12px">
            <div class="d-flex align-items-center mb-3">
              <input
                class="me-4"
                type="checkbox"
                :checked="props.kycInfos.bg1 == 0"
              />
              <div>YES/是</div>
            </div>
            <div class="d-flex align-items-center">
              <input
                class="me-4"
                type="checkbox"
                :checked="props.kycInfos.bg1 == 1"
              />
              <div>NO/否</div>
            </div>
          </td>
        </tr>

        <tr>
          <td colspan="2" class="kyc-label-col-2">
            <span
              >Do you have previous experience trading in a Stocks, Bonds,
              Contracts-for-difference, Forex, Derivatives, or other securities
              trading account?</span
            >
            <br />
            <span
              >您之前是否有过股票、债券、差价合约、外汇、衍生品或其他证券交易账户的交易经验？</span
            >
          </td>
          <td colspan="3" class="kyc-input-col-8" style="font-size: 12px">
            <div class="d-flex align-items-center mb-3">
              <input
                class="me-4"
                type="checkbox"
                :checked="props.kycInfos.bg2 == 0"
              />
              <div>YES/是</div>
            </div>
            <div class="d-flex align-items-center">
              <input
                class="me-4"
                type="checkbox"
                :checked="props.kycInfos.bg2 == 1"
              />
              <div>NO/否</div>
            </div>
          </td>
        </tr>

        <tr>
          <td colspan="2" class="kyc-label-col-2">
            <span>Are you an employee of MM or another Financial Company?</span>
            <br />
            <span>您是 MM 或其他金融公司的员工吗？</span>
          </td>
          <td colspan="3" class="kyc-input-col-8">
            <div style="font-size: 12px">
              <div class="d-flex align-items-center">
                <input
                  class="me-4"
                  type="checkbox"
                  :checked="props.kycInfos.exp1 == 0"
                />
                <div>
                  Yes Please list the previous financial firms where you have
                  been employeed / 是, 请列出您工作過的金融公司
                </div>
              </div>

              <div class="ms-4 mt-3">
                <ul>
                  <li class="mt-2">
                    Employer/公司 :
                    <span class="ms-3">{{ props.kycInfos.exp1Employer }}</span>
                  </li>
                  <li class="mt-2">
                    Position/職位 :
                    <span class="ms-3">{{ props.kycInfos.exp1Position }}</span>
                  </li>
                  <li class="mt-2">
                    Remuneration/报酬 :
                    <span class="ms-3">{{
                      props.kycInfos.exp1Remuneration
                    }}</span>
                  </li>
                </ul>
              </div>
              <div class="d-flex align-items-center">
                <input
                  class="me-4"
                  type="checkbox"
                  :checked="props.kycInfos.exp1 == 1"
                />
                <div>NO/否</div>
              </div>
            </div>
          </td>
        </tr>

        <tr>
          <td colspan="2" class="kyc-label-col-2">
            <span>Have you declared bankruptcy in the past 10 years?</span>
            <br />
            <span>您在过去 10 年内是否曾宣告破产？</span>
          </td>
          <td colspan="3" class="kyc-input-col-8">
            <div style="font-size: 12px">
              <div class="d-flex align-items-center">
                <input
                  class="me-4"
                  type="checkbox"
                  :checked="props.kycInfos.exp2 == 0"
                />
                <div>
                  Yes, please indicate the date of discharge and provide a copy
                  of the discharge letter with your application tip. /
                  是的，请注明出院日期并提供出院信副本以及您的申请提示。
                </div>
              </div>

              <div class="ms-8 mt-3 mb-3">
                Date:<span class="ms-3">{{ props.kycInfos.exp2More }}</span>
              </div>
              <div class="d-flex align-items-center">
                <input
                  class="me-4"
                  type="checkbox"
                  :checked="props.kycInfos.exp2 == 1"
                />
                <div>NO/否</div>
              </div>
            </div>
          </td>
        </tr>

        <tr>
          <td colspan="2" class="kyc-label-col-2">
            <span
              >Are you or any person having an interest in this account a member
              or employee of a commodity exchange or regulatory agency?</span
            >
            <br />
            <span
              >您或任何对此账户感兴趣的人是否是商品交易所或监管机构的成员或雇员？</span
            >
          </td>
          <td colspan="3" class="kyc-input-col-8">
            <div style="font-size: 12px">
              <div class="d-flex align-items-center mb-3">
                <input
                  class="me-4"
                  type="checkbox"
                  :checked="props.kycInfos.exp3 == 0"
                />
                <div>
                  Yes, please provide name(s) of Exchange or Agency. / 是,
                  请提供交易所或机构的名称:<span class="ms-3">{{
                    props.kycInfos.exp3More
                  }}</span>
                </div>
              </div>
              <div class="d-flex align-items-center">
                <input
                  class="me-4"
                  type="checkbox"
                  :checked="props.kycInfos.exp3 == 1"
                />
                <div>NO/否</div>
              </div>
            </div>
          </td>
        </tr>

        <tr>
          <td rowspan="2" class="kyc-label-col-2">
            <span>PEP</span>
            <br />
            <span>政治敏感人物</span>
          </td>
          <td class="kyc-input-col-3" style="font-size: 12px">
            Are you a Politically Exposed Person (PEP)?
            <br />您是政治公众人物 (PEP) 吗？
          </td>
          <td colspan="2" class="kyc-input-col-3">
            <div style="font-size: 12px">
              <div class="d-flex align-items-center mb-3">
                <input
                  class="me-4"
                  type="checkbox"
                  :checked="props.kycInfos.exp4 == 0"
                />
                <div>
                  Yes Please provide details/是, 请提供细节:<span
                    class="ms-3"
                    >{{ props.kycInfos.exp4More }}</span
                  >
                </div>
              </div>
              <div class="d-flex align-items-center">
                <input
                  class="me-4"
                  type="checkbox"
                  :checked="props.kycInfos.exp4 == 1"
                />
                <div>NO/否</div>
              </div>
            </div>
          </td>
        </tr>
        <tr>
          <td class="kyc-input-col-3" style="font-size: 12px">
            Are you an immediate family member or close associate of a PEP?
            <br />您是否有直系亲属或近亲属于政治敏感人物?
          </td>
          <td colspan="2" class="kyc-input-col-3">
            <div style="font-size: 12px">
              <div class="d-flex align-items-center mb-3">
                <input
                  class="me-4"
                  type="checkbox"
                  :checked="props.kycInfos.exp5 == 0"
                />
                <div>
                  Yes Please provide details/是, 请提供细节:<span
                    class="ms-3"
                    >{{ props.kycInfos.exp5More }}</span
                  >
                </div>
              </div>
              <div class="d-flex align-items-center">
                <input
                  class="me-4"
                  type="checkbox"
                  :checked="props.kycInfos.exp5 == 1"
                />
                <div>NO/否</div>
              </div>
            </div>
          </td>
        </tr>
      </table>

      <hr class="mt-11 mb-11" />

      <div class="mb-5">
        <strong>For Sole Trader Only</strong>/仅供个体经营者
      </div>

      <table style="width: 100%">
        <tr>
          <td class="kyc-label-col-2">
            <span>Full Business Name</span>
            <br />
            <span>公司全稱</span>
          </td>
          <td class="kyc-input-col-3">{{ props.kycInfos.businessName }}</td>
          <td class="kyc-label-col-2">
            <span>Business Number</span>
            <br />
            <span>商務號碼</span>
          </td>
          <td class="kyc-input-col-3">{{ props.kycInfos.businessNumber }}</td>
        </tr>

        <tr>
          <td class="kyc-label-col-2">
            <span>Full Business Address ( No PO Box )</span>
            <br />
            <span>辦公地址</span>
          </td>
          <td class="kyc-input-col-3">
            {{ props.kycInfos.businessAddress }}
          </td>
          <td colspan="2" class="kyc-input-col-3">
            <div class="d-flex" style="font-size: 12px">
              <div style="width: 50%">
                <div class="d-flex align-items-center">
                  <div>City/Suburb<br />城市</div>
                  <div class="ms-5 fs-3">{{ props.kycInfos.businessCity }}</div>
                </div>
                <div class="d-flex align-items-center">
                  <div>Zip/Postal<br />邮编</div>
                  <div class="ms-5 fs-3">
                    {{ props.kycInfos.businessZipCode }}
                  </div>
                </div>
              </div>

              <div style="width: 50%">
                <div class="d-flex align-items-center">
                  <div>State/Territory<br />州 / 省</div>
                  <div class="ms-5 fs-3">
                    {{ props.kycInfos.businessState }}
                  </div>
                </div>
                <div class="d-flex align-items-center">
                  <div>Country<br />国家</div>
                  <div class="ms-5 fs-3">
                    {{ props.kycInfos.businessCountry }}
                  </div>
                </div>
              </div>
            </div>
          </td>
        </tr>

        <tr>
          <td class="kyc-label-col-2">
            <span
              >Nature and purpose of the business relationship between MM and
              the client</span
            >
            <br />
            <span>MM与客户之间业务关系的性质和目的</span>
          </td>
          <td class="kyc-input-col-3">
            {{ props.kycInfos.businessCompanyPurposeBetweenUs }}
          </td>
          <td class="kyc-label-col-2">
            <span
              >Reason for the client requiring our services (incl. supporting
              documents, e.g. overseas suppliers’ invoices)</span
            >
            <br />
            <span
              >客户需要我方提供服务的原因(包括支持文件，例如海外供应商的发票)</span
            >
          </td>
          <td class="kyc-input-col-3">
            {{ props.kycInfos.businessServiceRequireReason }}
          </td>
        </tr>

        <tr>
          <td class="kyc-label-col-2">
            <span>Industry in which the client operates</span>
            <br />
            <span>客户经营所属行业</span>
          </td>
          <td class="kyc-input-col-3">{{ props.kycInfos.businessIndustry }}</td>
          <td class="kyc-label-col-2">
            <span>
              The main 3 industries in which the client’s customers
              operate</span
            >
            <br />
            <span>客户的客人所属的3个主要行业</span>
          </td>
          <td class="kyc-input-col-3">
            <div>
              <span class="me-3">1.</span
              >{{ props.kycInfos.businessClientIndustry1 }}
            </div>
            <div>
              <span class="me-3">2.</span
              >{{ props.kycInfos.businessClientIndustry2 }}
            </div>
            <div>
              <span class="me-3">3.</span
              >{{ props.kycInfos.businessClientIndustry3 }}
            </div>
          </td>
        </tr>

        <tr>
          <td class="kyc-label-col-2">
            <span
              >An explanation of a typical service provided by the client to its
              customers</span
            >
            <br />
            <span>客户向其客人提供的一项典型服务的详细描述</span>
          </td>
          <td colspan="3" class="kyc-input-col-8">
            {{ props.kycInfos.businessService }}
          </td>
        </tr>
      </table>

      <div class="mt-11">II. Verification/ 客户信息核实</div>
      <div class="mb-5">
        <div>
          At a minimum, CLIENTS’ FULL NAME and either their DATE OF BIRTH or
          RESIDENTIAL ADDRESS must be verified.
        </div>
        <div>最低限度,客户姓名和出生日期或住址必须核实.</div>
      </div>

      <table style="width: 100%">
        <tr>
          <td class="kyc-label-col-2">
            <span>Type of Document</span>
            <br />
            <span>⽂件类型</span>
          </td>
          <td class="kyc-input-col-3">
            <span v-if="props.kycInfos.idType == 1">Passport</span>
            <span v-if="props.kycInfos.idType == 2">Driver's License</span>
            <span v-if="props.kycInfos.idType == 3">Government ID</span>
          </td>
          <td class="kyc-label-col-2">
            <span>Document ID number</span>
            <br />
            <span>⽂件号码</span>
          </td>
          <td class="kyc-input-col-3">{{ props.kycInfos.idNumber }}</td>
        </tr>

        <tr>
          <td class="kyc-label-col-2">
            <span>Date of birth</span>
            <br />
            <span>出生日期</span>
          </td>
          <td class="kyc-input-col-3">{{ props.kycInfos.birthday }}</td>
          <td class="kyc-label-col-2">
            <span>Place of residence</span>
            <br />
            <span>居住地址</span>
          </td>
          <td class="kyc-input-col-3">{{ props.kycInfos.address }}</td>
        </tr>

        <tr>
          <td class="kyc-label-col-2">
            <span>Date of Issue</span>
            <br />
            <span>签发日期</span>
          </td>
          <td class="kyc-input-col-3">{{ props.kycInfos.idIssuedOn }}</td>
          <td class="kyc-label-col-2">
            <span>Expiry</span>
            <br />
            <span>过期日</span>
          </td>
          <td class="kyc-input-col-3">{{ props.kycInfos.idExpireOn }}</td>
        </tr>
      </table>

      <div class="mt-11 mb-5">Verification Method/ 核实方法</div>

      <table style="width: 100%">
        <tr>
          <td class="kyc-label-col-2">
            <span>Verification Method</span>
            <br />
            <span>核实方法</span>
          </td>
          <td colspan="3" class="kyc-input-col-8" style="font-size: 12px">
            <div class="d-flex align-items-center mb-3">
              <input
                class="me-2"
                type="checkbox"
                :checked="
                  props.kycInfos.electronicOrTraditional == 'traditional'
                "
              />
              <div>Traditional Verification/传统核实</div>
            </div>

            <div class="d-flex align-items-center">
              <input
                class="me-2"
                type="checkbox"
                :checked="
                  props.kycInfos.electronicOrTraditional == 'electronic'
                "
              />
              <div class="me-2">Electronic Verification/电子核实</div>
              (
              <div class="d-flex align-items-center ms-2">
                <input
                  class="me-2"
                  type="checkbox"
                  :checked="props.kycInfos.electronicSourceOption == 'labGroup'"
                />
                <div>LAB group</div>
              </div>
              <div class="d-flex align-items-center ms-2 me-2">
                <input
                  class="me-2"
                  type="checkbox"
                  :checked="props.kycInfos.electronicSourceOption == 'dataZoo'"
                />
                <div>Data Zoo</div>
              </div>

              <div class="d-flex align-items-center ms-2 me-2">
                <input
                  class="me-2"
                  type="checkbox"
                  :checked="
                    props.kycInfos.electronicSourceOption == 'refinitiv'
                  "
                />
                <div>Refinitiv</div>
              </div>
              )
            </div>
          </td>
        </tr>
      </table>
      <table class="mt-7" style="width: 100%">
        <tr class="text-center fw-bold">
          <td class="kyc-label-col-5" style="font-size: 14px">
            <span>Traditional Verification/传统核实</span>
          </td>
          <td class="kyc-label-col-5" style="font-size: 14px">
            <span>Electronic Verification/电子核实</span>
          </td>
        </tr>

        <tr>
          <td class="kyc-input-col-5" style="font-size: 12px">
            <div class="d-flex align-items-center">
              <input
                class="me-2"
                type="checkbox"
                :checked="props.kycInfos.traditionalOption == 'traditional1'"
              />
              <strong>Verification method 1/核实方法1</strong>
            </div>
            <div class="ms-6">
              <div>
                Original or certified copy of a primary photographic
                identification document/
              </div>
              <div>首要的含照片身份证明文件的原始件或经认证的复印/</div>
            </div>

            <div class="d-flex align-items-center mt-3">
              <input
                class="me-2"
                type="checkbox"
                :checked="props.kycInfos.traditionalOption == 'traditional2'"
              />
              <strong>Verification method 2/核实方法2</strong>
            </div>
            <div class="ms-6">
              <div>
                Original or certified copy of a primary non-photographic
                identification document; AND/OR
              </div>
              <div>
                首要的不含照片的身份证明文件原始件或经认证的复印件; 以及/或
              </div>
              <div>
                Original or certified copy of a secondary identification
                document;
              </div>
              <div>二级身份证明文件的原始件或经认证的复印;</div>
            </div>
          </td>
          <td class="kyc-input-col-5" style="font-size: 12px">
            <div class="mb-3">
              <strong
                >Verify information collected about a client from:
                <br />收集的关于核实客户信息来源于:</strong
              >
            </div>
            <div class="d-flex align-items-center">
              <input
                class="me-2"
                type="checkbox"
                :checked="props.kycInfos.electronicOption == '0'"
              />
              <div class="ms-3">
                Reliable and independent documentation
                <br />可靠且独立的文件
              </div>
            </div>

            <div class="d-flex align-items-center mt-3">
              <input
                class="me-2"
                type="checkbox"
                :checked="props.kycInfos.electronicOption == '1'"
              />
              <div class="ms-3">
                <div>
                  Reliable and independent electronic data from at least one (1)
                  separate data source <br />来自至少一 (1)
                  个单独数据源的可靠且独立的电子
                </div>
              </div>
            </div>

            <div class="d-flex align-items-center mt-3">
              <input
                class="me-2"
                type="checkbox"
                :checked="props.kycInfos.electronicOption == '2'"
              />
              <div class="ms-3">
                <div>A combination of both of the above <br />上面两者结合</div>
              </div>
            </div>
          </td>
        </tr>
      </table>

      <table class="mt-7" style="width: 100%">
        <tr>
          <td class="kyc-label-col-2">
            <span>Compliance Notes</span>
            <br />
            <span>合规部门备注</span>
          </td>
          <td
            colspan="3"
            class="kyc-input-col-8"
            style="font-size: 16px; color: #900000"
          >
            {{ props.kycInfos.complianceNote }}
          </td>
        </tr>
      </table>

      <div class="mt-12">
        <div class="mb-5" style="font-weight: bold">
          <div>
            Where a client cannot provide satisfactory evidence of identity:
          </div>
          <div>当客户无法提供令人满意的身份证明时：</div>
        </div>
        <div class="mb-5">
          <div>
            Where a client does not possess and is unable to obtain the
            necessary information or evidence of identity, then in limited and
            exceptional cases, we may use alternative identity proofing process,
            which are commensurate with our risk-based systems and controls.
          </div>
        </div>
        <div class="mb-5">
          <div>
            In these limited and exceptional cases, the matter will be referred
            to the AML/CTF compliance officer who may accept a self-attestation
            from the client, if they are not aware that the self-attestation is
            incorrect or misleading. The AML/CTF compliance officer will also
            designate the client as being high risk and will implement the
            appropriate level of ongoing customer due diligence procedures which
            is commensurate with a high risk client.
          </div>
        </div>
        <div class="mb-5">
          <div>
            如果客户不拥有且无法获得必要的身份信息或证明，那么在有限和例外的情况下，我们可能会使用替代性的身份证明流程，该流程与基于风险的系统和控制措施相符。
          </div>
        </div>
        <div class="mb-5">
          <div>
            在这些有限和例外的情况下，此事将转给可能接受客户自我证明的AML /
            CTF合规官員, 如果他们不知道该自我证明是不正确或具有误导性。AML /
            CTF合规官还将指定客户为高风险客户，并将实施与高风险客户相对应的适当级别的持续客户尽职调查程序。
          </div>
        </div>
      </div>
    </div>

    <div class="mt-11 mb-9" style="position: relative">
      <hr />
      <div
        class="text-center"
        style="
          position: absolute;
          top: 50%;
          left: 50%;
          transform: translate(-50%, -50%);
          background-color: white;
          padding: 0px 10px 0px 18px;
        "
      >
        <strong>For Office Use Only/本公司专用（请勿填写）</strong>
      </div>
    </div>

    <div class="d-flex justify-content-around">
      <table style="width: 48%">
        <tr>
          <td class="kyc-label-col-2">
            <span>Admin Staff Member</span>
            <br />
            <span>办公室员工</span>
          </td>
          <td class="kyc-input-col-3">{{ props.kycInfos.staffName }}</td>
        </tr>

        <tr>
          <td class="kyc-label-col-2">
            <span>Position</span>
            <br />
            <span>职位</span>
          </td>
          <td class="kyc-input-col-3">{{ props.kycInfos.staffPosition }}</td>
        </tr>

        <tr>
          <td class="kyc-label-col-2">
            <span>Signature</span>
            <br />
            <span>签名</span>
          </td>
          <td class="kyc-input-col-3">
            <img :src="props.kycInfos.staffSignature" height="80" alt="" />
          </td>
        </tr>

        <tr>
          <td class="kyc-label-col-2">
            <span>Date Signed</span>
            <br />
            <span>签字日期</span>
          </td>
          <td class="kyc-input-col-3">{{ props.kycInfos.staffSignedOn }}</td>
        </tr>
      </table>

      <table style="width: 48%">
        <tr>
          <td class="kyc-label-col-2">
            <span>Compliance Staff Member</span>
            <br />
            <span>合规部门员工</span>
          </td>
          <td class="kyc-input-col-3">{{ props.kycInfos.complianceName }}</td>
        </tr>

        <tr>
          <td class="kyc-label-col-2">
            <span>Position</span>
            <br />
            <span>职位</span>
          </td>
          <td class="kyc-input-col-3">
            {{ props.kycInfos.compliancePosition }}
          </td>
        </tr>

        <tr>
          <td class="kyc-label-col-2">
            <span>Signature</span>
            <br />
            <span>签名</span>
          </td>
          <td class="kyc-input-col-3">
            <img :src="props.kycInfos.complianceSignature" height="80" alt="" />
          </td>
        </tr>

        <tr>
          <td class="kyc-label-col-2">
            <span>Date Signed</span>
            <br />
            <span>签字日期</span>
          </td>
          <td class="kyc-input-col-3">
            {{ props.kycInfos.complianceSignedOn }}
          </td>
        </tr>
      </table>
    </div>

    <hr class="mt-11 mb-11" />

    <div class="mb-5">
      <div>COPY OF SUPPORTING DOCUMENTATION</div>
      <div>支持性⽂件复印件</div>
    </div>

    <table width="100%">
      <tr>
        <td class="kyc-label-col-2">
          <span>Type of Document</span>
          <br />
          <span>⽂件类型</span>
        </td>
        <td colspan="3" class="kyc-input-col-8" style="font-size: 12px">
          <div class="d-flex align-items-center mb-3">
            <input
              class="me-2"
              type="checkbox"
              :checked="props.kycInfos.idType == '3'"
            />
            <div>Passport (Country of Origin)/护照（原籍国）</div>
          </div>

          <div class="d-flex align-items-center mb-3">
            <input
              class="me-2"
              type="checkbox"
              :checked="props.kycInfos.idType == '2'"
            />
            <div>
              Photo ID / DL (Country of Origin)/含照片身份证件/驾照（原籍国）
            </div>
          </div>

          <div class="d-flex align-items-center">
            <input
              class="me-2"
              type="checkbox"
              :checked="props.kycInfos.idType == '1'"
            />
            <div>
              Financial Statement/Utility Bill/Government Document/
              财务证明/水电费账单/政府文件
            </div>
          </div>
        </td>
      </tr>
    </table>

    <hr class="mt-11 mb-11" />

    <div class="my-8">
      <div class="text-xs font-semibold mb-6 tracking-widerr">
        <div>
          You must verify the CLIENT’S FULL NAME and either their DATE OF BIRTH
          or RESIDENTIAL ADDRESS using:
        </div>
        <div>您必须利用以下资料确认客户姓名和出生日期或住址信息:</div>
      </div>
      <div class="p-4" style="border: 1px solid lightgray">
        <div class="text-xs font-semibold mb-4 tracking-widerr">
          <div>
            AN ORIGINAL OR CERTIFIED COPY OF A PRIMARY PHOTOGRAPHIC
            IDENTIFICATION DOCUMENT
          </div>
          <div>一份含照⽚的首要身份证明⽂件原始件或认证的复印件</div>
        </div>
        <div class="px-8">
          <ul class="list-disc list-outside text-3xs">
            <li class="mb-2">
              <div>
                A license or permit issued under a law of a State or Territory
                or equivalent authority of a foreign country for the purpose of
                driving a vehicle that contains a photograph of the person in
                whose name the document is issued;
              </div>
              <div>
                州或领地或其他外国的政府或同等权威机构颁发的含申请人姓名及照⽚的用于驾驶车辆的许可证件;
              </div>
            </li>
            <li class="mb-2">
              <div>a passport issued by the Commonwealth;</div>
              <div>联邦颁发的护照;</div>
            </li>
            <li class="mb-2">
              <div>
                a passport or a similar document issued for the purpose of
                international travel, that:
              </div>
              <div>护照或其他和护照性质一样用于国际旅行的⽂件:</div>
              <div class="px-8">
                <ol class="list-decimal list-outside text-3xs">
                  <li>
                    <div>
                      Contains a photograph and the signature of the person in
                      whose the document is issued;
                    </div>
                    <div>签发的⽂件需包含申请人照⽚和签名;</div>
                  </li>
                  <li>
                    <div>
                      is issued by a foreign government, the United Nations or
                      an agency of the United Nations; and
                    </div>
                    <div>由外国政府, 联合国或联合国所属机构颁发, 以及</div>
                  </li>
                  <li>
                    <div>
                      if it is written in a language that is not understood by
                      the person carrying out the the verification – is
                      accompanied by an English translation prepared by an
                      accredit translator.
                    </div>
                    <div>
                      如果进行验证的人对⽂件所用语言并不了解或认知,需由授权翻译机构翻译成英⽂.
                    </div>
                  </li>
                </ol>
              </div>
            </li>
            <li class="mb-2">
              <div>
                A card issued under the law of a State or Territory for the
                purpose of proving the person’s age which contains a photograph
                of the person in whose name the document is issued.
              </div>
              <div>
                按照州或领地法律颁发的用于证明申请人年龄的包含申请人照⽚及姓名的证件.
              </div>
            </li>
            <li class="mb-2">
              <div>
                A national identity card issued for the purpose of
                identification, that:
              </div>
              <div>用于识别身份的国民身份证:</div>
              <div class="px-8">
                <ol class="list-decimal list-outside text-3xs mx-2">
                  <li>
                    <div>
                      Contains a photograph and a signature of the person in
                      whose name the document is issued;
                    </div>
                    <div>包含申请人照⽚和签名;</div>
                  </li>
                  <li>
                    <div>
                      is issued by a foreign government, the United Nations or
                      an agency of the United Nations if it is written in a
                      language that is not understood by the person carrying out
                      the verification – is accompanied by an English
                      translation prepared by an accredit translator.
                    </div>
                    <div>
                      由外国政府, 联合国或联合国所属机构颁发,
                      如进行验证的人对⽂件所用语言并不了解或认知,
                      需由授权翻译机构翻译成英⽂.
                    </div>
                  </li>
                </ol>
              </div>
            </li>
          </ul>
        </div>
      </div>
    </div>

    <div class="text-xs font-semibold text-center w-full tracking-widerr mb-7">
      OR / 或
    </div>

    <div class="p-4" style="border: 1px solid lightgray">
      <div class="text-xs font-semibold mb-4 tracking-widerr">
        <div>
          AN ORIGINAL OR CERTIFIED COPY OF A PRIMARY PHOTOGRAPHIC IDENTIFICATION
          DOCUMENT
        </div>
        <div>一份含照⽚的首要身份证明⽂件原始件或认证的复印件</div>
      </div>
      <div class="px-8">
        <ul class="list-disc list-outside text-3xs">
          <li class="mb-2">
            <div>
              A birth certificate or birth extract issued by a State or
              Territory;
            </div>
            <div>由州或领地颁发的出生证明或出生信息摘要;</div>
          </li>
          <li class="mb-2">
            <div>a citizenship certificate issued by the Commonwealth;</div>
            <div>由联邦颁发的公民证明;</div>
          </li>
          <li class="mb-2">
            <div>
              a citizenship certificate issued by a foreign government that, if
              it is written in a language that is not understood by the person
              carrying out the verification, is accompanied by an English
              translation prepared by an accredited translator;
            </div>
            <div>
              由外国政府颁发的公民证明,
              如进行验证的人对⽂件所用语言并不了解或认知,
              需由授权翻译机构翻译成英⽂.
            </div>
          </li>
          <li class="mb-2">
            <div>
              a birth certificate issued by a foreign government, the United
              Nations or an agency of the United Nations that, if it is written
              in a language that is not understood by the person carrying out
              the verification, is accompanied by an English translation
              prepared by an accredited translator;
            </div>
            <div>
              由外国政府,联合国或联合国所属机构颁发的出生证明,
              如进行验证的人对⽂件所用语言并不了解或认知,
              需由授权翻译机构翻译成英⽂;
            </div>
          </li>
          <li class="mb-2">
            <div>
              a pension card issued by Centrelink that entitles the person in
              whose name the card is issued, to financial benefits.
            </div>
            <div>
              由centrelink发出的, 基于财务效益,
              授予申请人的包含申请人姓名的养⽼金卡.
            </div>
          </li>
        </ul>
      </div>

      <div
        class="text-xs font-semibold text-center w-full my-8 tracking-widerr"
      >
        AND / 以及
      </div>

      <div class="text-xs font-semibold mb-4 tracking-widerr">
        <div>
          AN ORIGINAL OR CERTIFIED COPY OF A SECONDARY IDENTIFICATION DOCUMENT
        </div>
        <div>一份含照⽚的二级身份证明⽂件原始件或认证的复印件</div>
      </div>
      <div class="px-8">
        <ul class="list-disc list-outside text-3xs">
          <li class="mb-2">
            <div>A notice that / 一份声明:</div>
            <div class="px-8">
              <ol class="list-decimal list-outside text-3xs">
                <li>
                  <div>
                    was issued to an individual by the Commonwealth, a State or
                    Territory within the preceding twelve months;
                  </div>
                  <div>由聯邦或州或領地在前12個月內簽發給個人的;</div>
                </li>
                <li>
                  <div>
                    contains the name of the individual and his or her
                    residential address; and
                  </div>
                  <div>包含個人姓名和他/她的住址信息; 以及</div>
                </li>
                <li>
                  <div>
                    records the provision of financial benefits to the
                    individual under a law of the Commonwealth, State or
                    Territory (as the case may be);
                  </div>
                  <div>
                    根據聯邦或州或領地法律提供個人財務收益的記錄(視情況而定);
                  </div>
                </li>
              </ol>
            </div>
          </li>
          <li class="mb-2">
            <div>A notice that / 一份声明:</div>
            <div class="px-8">
              <ol class="list-decimal list-outside text-3xs">
                <li>
                  <div>
                    was issued to an individual by the Australian Taxation
                    Office within the preceding 12 months;
                  </div>
                  <div>由澳大利亞稅務機構在前12個月內簽發給個人的;</div>
                </li>
                <li>
                  <div>
                    contains the name of the individual and his or her
                    residential address; and
                  </div>
                  <div>包含個人姓名和他/她的住址信息; 以及</div>
                </li>
                <li>
                  <div>
                    records a debt payable to or by the individual by or to
                    (respectively) the Commonwealth under Commonwealth law
                    relating to taxation;
                  </div>
                  <div>根據聯邦稅務相關法律關於個人債務支付的記錄;</div>
                </li>
              </ol>
            </div>
          </li>
          <li class="mb-2">
            <div>A notice that / 一份声明:</div>
            <div class="px-8">
              <ol class="list-decimal list-outside text-3xs">
                <li>
                  <div>
                    was issued to an individual by a local government body or
                    utilities provider within the preceding three months;
                  </div>
                  <div>由當地政府或公共事業機構在前3個月內簽發給個人的;</div>
                </li>
                <li>
                  <div>
                    contains the name of the individual and his or her
                    residential address; and
                  </div>
                  <div>包含個人姓名和他/她的住址信息; 以及</div>
                </li>
                <li>
                  <div>
                    records the provision of services by that local government
                    body or utilities provider to that address or to that
                    person.
                  </div>
                  <div>
                    由當地政府機構或公共事業機構對該住址或個人提供的服務記錄.
                  </div>
                </li>
              </ol>
            </div>
          </li>
          <li class="mb-2">
            <div>A notice that / 一份声明:</div>
            <div class="px-8">
              <ol class="list-decimal list-outside text-3xs">
                <li>
                  <div>
                    was issued to a person by a school principal within the
                    preceding three months;
                  </div>
                  <div>由學校校長在前3個月之內簽發給個人的；</div>
                </li>
                <li>
                  <div>
                    contains the name of the person and his or her residential
                    address; and
                  </div>
                  <div>包含個人姓名和他/她的住址信息; 以及</div>
                </li>
                <li>
                  <div>
                    records the period of time that the person attended at the
                    school.
                  </div>
                  <div>提供個人在校時間的記錄.</div>
                </li>
              </ol>
            </div>
          </li>
        </ul>
      </div>
    </div>

    <footer class="text-center mt-11">
      <p>L1, 6-10 O’Connell Street Sydney, NSW 2000 Australia</p>
      <p>MM Co Ltd | AU Company No. | License No.</p>
    </footer>
  </div>
</template>
<script setup lang="ts">
import { ref, watch } from "vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const props = defineProps<{
  kycInfos: any;
}>();

// watch(
//   () => props.kycInfos,
//   () => {
//     item.value = {
//       ...props.verificationDetails?.financial,
//     };
//   }
// );
</script>

<style scoped>
.kyc-label-col-2 {
  width: 20%;
  padding: 8px;
  border: 1px solid black;
  background-color: #e9e9e9;

  font-size: 10px;
}

.kyc-input-col-3 {
  width: 30%;

  padding: 8px;
  border: 1px solid black;

  font-size: 15px;
}
.kyc-label-col-5 {
  width: 50%;
  padding: 8px;
  border: 1px solid black;
  background-color: #e9e9e9;

  font-size: 10px;
}

.kyc-input-col-5 {
  width: 50%;

  padding: 20px;
  border: 1px solid black;

  font-size: 15px;
}

.kyc-input-col-8 {
  width: 80%;

  padding: 8px;
  border: 1px solid black;

  font-size: 15px;
}
</style>
