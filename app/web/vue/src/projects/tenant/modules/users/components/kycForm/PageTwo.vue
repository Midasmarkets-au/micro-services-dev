<template>
  <div class="page-content">
    <div class="flex flex-row base-row-height border border-black-800">
      <div class="flex-1 flex flex-row">
        <div class="field">
          <div>
            Are you or any person having an interest in this account a member or
            employee of a commodity exchange or regulatory agency?
          </div>
          <div>
            您或对此帐户感兴趣的其他任何人是否是商品交易所或监管机构的成员或雇员？
          </div>
        </div>
      </div>
      <div class="flex-1 flex-col ml-2">
        <div class="flex flex-row">
          <div class="checkbox-wrapper">
            <input
              id="commodity-exchange-yes"
              class="checkbox"
              type="checkbox"
              name="commodity-exchange"
              :checked="
                Number.isInteger(value.interest_account) &&
                value.interest_account == 1
              "
              disabled
            />
            <label for="commodity-exchange-yes"></label>
          </div>
          <div class="ml-3">
            <div class="field">
              If Yes, please provide the name(s) of the exchange or agency:
            </div>
            <div class="field">如果是，请提供交易所或代理商的名称:</div>
            <div class="field-data relative h-5">
              <span class="mb-1">{{
                Number.isInteger(value.interest_account) &&
                value.interest_account == 1
                  ? value.interest_account_note
                  : ""
              }}</span>
              <span class="bottom-0 absolute"
                >___________________________________________</span
              >
            </div>
          </div>
        </div>
        <div class="flex flex-row">
          <div class="checkbox-wrapper">
            <input
              id="commodity-exchange-no"
              class="checkbox"
              type="checkbox"
              name="commodity-exchange"
              :checked="
                Number.isInteger(value.interest_account) &&
                value.interest_account == 0
              "
              disabled
            />
            <label for="commodity-exchange-no"></label>
          </div>
          <div class="field ml-3">No/否</div>
        </div>
      </div>
    </div>

    <div
      class="flex flex-row double-half-row-height border border-black-800 border-t-0"
    >
      <div class="flex-1 flex flex-row">
        <div class="field">
          <div>Are you an employee of MM or other Financial Company?</div>
          <div>您是否为MM的员工或其他金融公司的员工？</div>
        </div>
      </div>
      <div class="flex-1 flex-col ml-2">
        <div class="flex flex-row">
          <div class="checkbox-wrapper">
            <input
              id="bcr-employee-yes"
              class="checkbox"
              type="checkbox"
              name="bcr-employee"
              :checked="
                Number.isInteger(value.work_for_other_financial_firm) &&
                value.work_for_other_financial_firm == 1
              "
              disabled
            />
            <label for="bcr-employee-yes"></label>
          </div>
          <div class="ml-3">
            <div class="field flex flex-row">
              <div>Yes</div>
              <div class="ml-3">Please provide details/是, 请提供细节:</div>
            </div>
          </div>
        </div>
        <div class="flex flex-row items-end">
          <div class="field">Employer/雇主:</div>
          <div class="field-data relative h-5">
            <span class="mb-1">{{
              Number.isInteger(value.work_for_other_financial_firm) &&
              value.work_for_other_financial_firm == 1
                ? value.other_financial_firm_employer
                : ""
            }}</span>
            <span class="bottom-0 absolute">
              _________________________________</span
            >
          </div>
        </div>
        <div class="flex flex-row items-end">
          <div class="field">Job Description/工作描述:</div>
          <div class="field-data relative h-5">
            <span class="mb-1">{{
              Number.isInteger(value.work_for_other_financial_firm) &&
              value.work_for_other_financial_firm == 1
                ? value.other_financial_firm_position
                : ""
            }}</span>
            <span class="bottom-0 absolute">
              _________________________________</span
            >
          </div>
        </div>
        <div class="flex flex-row items-end">
          <div class="field">Remuneration/报酬:</div>
          <div class="field-data relative h-5">
            <span class="mb-1">{{
              Number.isInteger(value.work_for_other_financial_firm) &&
              value.work_for_other_financial_firm == 1
                ? value.other_financial_firm_remuneration
                : ""
            }}</span>
            <span class="bottom-0 absolute">
              _________________________________</span
            >
          </div>
        </div>
        <div class="flex flex-row">
          <div class="checkbox-wrapper">
            <input
              id="bcr-employee-no"
              class="checkbox"
              type="checkbox"
              name="bcr-employee"
              :checked="
                Number.isInteger(value.work_for_other_financial_firm) &&
                value.work_for_other_financial_firm == 0
              "
              disabled
            />
            <label for="bcr-employee-no"></label>
          </div>
          <div class="field ml-3">No/否</div>
        </div>
      </div>
    </div>

    <!-- <div class="flex flex-row double-row-height border border-black-800 border-t-0">
            <div class="flex-1 flex flex-row">
                <div class="field">
                    <div>
                        Have you declared bankruptcy in the past 10 years?
                    </div>
                    <div>
                        您在过去十年中宣布破产了吗？
                    </div>
                </div>
            </div>
            <div class="flex-1 flex-col ml-2">
                <div class="flex flex-row">
                    <div class="checkbox-wrapper">
                        <input 
                            id="bankruptcy-yes"
                            class="checkbox" 
                            type="checkbox" 
                            name="bankruptcy" 
                            :checked="Number.isInteger(value.bankrupt) && value.bankrupt==1" 
                            disabled
                        />
                        <label for="bankruptcy-yes"></label>
                    </div>
                    <div class="ml-3">
                        <div class="field">
                            Yes please indicate date of discharge and provide a copy of the discharge letter with your application documents/有, 请注明破产解除⽇期，并提供您的破产解除信复印件以及您的申请文件:
                        </div>
                        <div class="field-data relative h-5">
                            <span class="mb-1">{{Number.isInteger(value.bankrupt) && value.bankrupt==1 ? value.bankrupt_note : ''}}</span>
                            <span class="bottom-0 absolute">___________________________________________</span>
                        </div>
                    </div>
                </div>
                <div class="flex flex-row">
                    <div class="checkbox-wrapper">
                        <input 
                            id="bankruptcy-no" 
                            class="checkbox" 
                            type="checkbox" 
                            name="bankruptcy" 
                            :checked="Number.isInteger(value.bankrupt) && value.bankrupt==0" 
                            disabled
                        />
                        <label for="bankruptcy-no"></label>
                    </div>
                    <div class="field ml-3">
                        No/否
                    </div>
                </div>
            </div>
        </div> -->

    <hr class="w-full border-black-800 border-t my-4" />

    <div class="mb-6">
      <div class="text-2xs font-semibold mb-2 tracking-widerr">
        For Sole Traders Only/仅供个体经营者
      </div>
      <div class="w-full">
        <div class="flex flex-row base-row-height border border-black-800">
          <div class="flex-1 flex flex-row border-r border-black-800">
            <div class="field w-1/3 border-r border-black-800">
              <div>Full Business Name:</div>
              <div>公司全称:</div>
            </div>
            <div class="field-data w-2/3">
              {{ value.company_name }}
            </div>
          </div>
          <div class="flex-1 flex flex-row">
            <div class="field w-1/3 border-r border-black-800">
              <div>Business Number:</div>
              <div>商务号码:</div>
            </div>
            <div class="field-data w-2/3">
              {{ value.company_abn }}
            </div>
          </div>
        </div>

        <div
          class="flex flex-row one-half-row-height border border-black-800 border-t-0"
        >
          <div class="flex-1 flex flex-row">
            <div class="field w-1/3">
              <div>Full Street Business Address –No PO Box:</div>
              <div>(if different to residential address)</div>
              <div>办公地址:</div>
            </div>
            <div class="field-data w-2/3">
              {{ value.company_address }}
            </div>
          </div>
          <div class="flex-1 flex flex-col">
            <div class="flex-1 flex flex-row">
              <div class="flex-grow flex flex-row">
                <div class="field">
                  <div>City/Suburb:</div>
                  <div>城市:</div>
                </div>
                <div class="field-data flex-grow whitespace-nowrap">
                  {{ value.company_city }}
                </div>
              </div>
              <div class="w-2/3 flex flex-row">
                <div class="flex-1 flex flex-row">
                  <div class="field">
                    <div>State/Territory:</div>
                    <div>州 / 省:</div>
                  </div>
                  <div class="field-data flex-grow whitespace-nowrap">
                    {{ value.company_state }}
                  </div>
                </div>
              </div>
            </div>
            <div class="flex-1 flex flex-row">
              <div class="flex-grow flex flex-row">
                <div class="field">
                  <div>Zip/Postal:</div>
                  <div>邮编:</div>
                </div>
                <div class="field-data flex-grow whitespace-nowrap">
                  {{ value.company_zipcode }}
                </div>
              </div>
              <div class="w-2/3 flex flex-row">
                <div class="flex-1 flex flex-row">
                  <div class="field">
                    <div>Country:</div>
                    <div>国家:</div>
                  </div>
                  <div class="field-data flex-grow">
                    {{ value.company_country }}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div
          class="flex flex-row double-row-height border border-black-800 border-t-0"
        >
          <div class="flex-1 flex flex-row border-r border-black-800">
            <div class="field w-1/3 border-r border-black-800">
              <div>
                Nature and purpose of the business relationship between MM and
                the client:
              </div>
              <div>MM与客户之间业务关系的性质和目的:</div>
            </div>
            <div class="field-data w-2/3">
              {{ value.company_purpose_between_bcr }}
            </div>
          </div>
          <div class="flex-1 flex flex-row">
            <div class="field w-1/3 border-r border-black-800">
              <div>
                Reason for the client requiring our services (incl. supporting
                documents, e.g. overseas suppliers’ invoices):
              </div>
              <div>
                客户需要我方提供服务的原因(包括支持文件，例如海外供应商的发票):
              </div>
            </div>
            <div class="field-data w-2/3">
              {{ value.company_needs_bcr_service_reason }}
            </div>
          </div>
        </div>

        <div
          class="flex flex-row one-half-row-height border border-black-800 border-t-0"
        >
          <div class="flex-1 flex flex-row border-r border-black-800">
            <div class="field w-1/3 border-r border-black-800">
              <div>Industry in which the client operates:</div>
              <div>客户经营所属行业:</div>
            </div>
            <div class="field-data w-2/3">
              {{ value.company_instury }}
            </div>
          </div>
          <div class="flex-1 flex flex-row">
            <div class="field w-1/3 border-r border-black-800">
              <div>
                The main 3 industries in which the client’s customers operate:
              </div>
              <div>客户的客人所属的3个主要行业:</div>
            </div>
            <div class="field-data w-2/3">
              <ol class="list-decimal list-inside text-2xs">
                <li>{{ value.company_clinet_operate_industry_first }}</li>
                <li>{{ value.company_clinet_operate_industry_second }}</li>
                <li>{{ value.company_clinet_operate_industry_third }}</li>
              </ol>
            </div>
          </div>
        </div>

        <div
          class="flex flex-row base-row-height border border-black-800 border-t-0"
        >
          <div class="field">
            <div>
              An explanation of a typical service provided by the client to its
              customers:
            </div>
            <div>客户向其客人提供的一项典型服务的详细描述：</div>
          </div>
          <div class="field-data">
            {{ value.company_service_to_client }}
          </div>
        </div>
      </div>
    </div>

    <div>
      <div class="text-sm font-semibold mb-4">
        <div class="mb-1 tracking-widerr">II. Verification/ 客户信息核实</div>
        <div class="text-2xs font-normal">
          <p>
            At a minimum, CLIENTS’ FULL NAME and either their DATE OF BIRTH or
            RESIDENTIAL ADDRESS must be verified.
          </p>
          <p>最低限度,客户姓名和出生日期或住址必须核实.</p>
        </div>
      </div>

      <div class="w-full">
        <div class="flex flex-row base-row-height border border-black-800">
          <div class="flex-1 flex flex-row border-r border-black-800">
            <div class="field w-1/3 border-r border-black-800">
              <div>Type of Document:</div>
              <div>⽂件类型:</div>
            </div>
            <div class="field-data w-2/3">
              {{ value.form_of_id }}
            </div>
          </div>
          <div class="flex-1 flex flex-row">
            <div class="field w-1/3 border-r border-black-800">
              <div>Document ID number:</div>
              <div>⽂件号码:</div>
            </div>
            <div class="field-data w-2/3">
              {{ value.id_number }}
            </div>
          </div>
        </div>

        <div
          class="flex flex-row base-row-height border border-black-800 border-t-0"
        >
          <div class="flex-1 flex flex-row border-r border-black-800">
            <div class="field w-1/3 border-r border-black-800">
              <div>Date of birth:</div>
              <div>出生日期:</div>
            </div>
            <div class="field-data w-2/3">
              {{ value.date_of_birth }}
            </div>
          </div>
          <div class="flex-1 flex flex-row">
            <div class="field w-1/3 border-r border-black-800">
              <div>Place of residence:</div>
              <div>居住地址:</div>
            </div>
            <div class="field-data w-2/3">
              {{ value.full_address }}
            </div>
          </div>
        </div>

        <div
          class="flex flex-row base-row-height border border-black-800 border-t-0"
        >
          <div class="flex-1 flex flex-row border-r border-black-800">
            <div class="field w-1/3 border-r border-black-800">
              <div>Date of Issue:</div>
              <div>签发日期:</div>
            </div>
            <div class="field-data w-2/3">
              {{ value.id_issued_date }}
            </div>
          </div>
          <div class="flex-1 flex flex-row">
            <div class="field w-1/3 border-r border-black-800">
              <div>Expiry:</div>
              <div>过期日:</div>
            </div>
            <div class="field-data w-2/3">
              {{ value.id_expiration_date }}
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
export default {
  props: ["value"],
};
</script>
<style scoped>
#new-kyc-form body {
  margin: 0px 0px 0px 0px !important;
  padding: 0px 0px 0px 0px !important;
  width: 800px;
  box-sizing: inherit;
  border-width: 0;
  border-style: solid;
  border-color: rgba(0, 0, 0, 0.8);
  font-family: "Microsoft-Yahei";
  color: rgba(0, 0, 0, 0.8);
  font-size: 0.75rem;
  counter-reset: page;
  background-color: white;
}

header {
  position: running(header);
  font-size: 0.75rem;
  color: rgba(0, 0, 0, 0.8);
}

footer {
  position: running(footer);
  text-align: center;
  font-size: 0.75rem;
  color: rgba(0, 0, 0, 0.8);
}

.page {
  padding-right: 0.5rem;
  padding-left: 0.5rem;
  page-break-before: always;
  height: 27.94cm;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  color: rgba(0, 0, 0, 0.8);
}

div.page .page-number {
  counter-increment: page;
}

div.page .page-number:after {
  display: block;
  text-align: right;
  content: "Page " counter(page) " of 5";
  margin-bottom: 0.25in;
}

.page-content {
  padding-top: 4.5rem;
  color: rgba(0, 0, 0, 0.8);
}

.relative {
  position: relative;
}

.absolute {
  position: absolute;
}

.top-0 {
  top: 0;
}

.right-0 {
  right: 0;
}

.bottom-0 {
  bottom: 0;
}

.left-0 {
  left: 0;
}

table {
  page-break-inside: auto;
}

.base-row-height {
  min-height: 2.5rem;
}

.one-half-row-height {
  min-height: 3.75rem;
}

.double-row-height {
  min-height: 5rem;
}

.double-half-row-height {
  min-height: 6.25rem;
}

.triple-row-height {
  min-height: 7.5rem;
}

.triple-half-row-height {
  min-height: 8.75rem;
}

.quadruple-row-height {
  min-height: 10rem;
}

.field {
  padding-top: 0.25rem;
  padding-bottom: 0.25rem;
  padding-right: 0.5rem;
  padding-left: 0.5rem;
  line-height: 1.375;
  font-size: 0.6rem;
  letter-spacing: -0.02em;
  color: rgba(0, 0, 0, 0.8);
}

.field-data {
  font-size: 0.825rem;
  display: flex;
  align-items: center;
  padding-right: 0.5rem;
  padding-left: 0.5rem;
  padding-top: 0.25rem;
  padding-bottom: 0.25rem;
  letter-spacing: 0;
  color: rgba(0, 0, 0) !important;
}

.text-2xs {
  font-size: 0.7rem;
}

.text-3xs {
  font-size: 0.6rem;
}

ol,
ul,
li {
  margin-left: 0;
  padding-left: 0;
}

.list-disc {
  list-style-type: disc;
}

.list-decimal {
  list-style-type: decimal;
}

.list-inside {
  list-style-position: inside;
}

.list-outside {
  list-style-position: outside;
}

.text-black-800 {
  color: rgba(0, 0, 0, 0.8);
}

.border-black-800 {
  border-color: rgba(0, 0, 0, 0.8);
}

.checkbox-wrapper {
  position: relative;
  margin-top: 0.3rem;
}

.checkbox + label {
  display: block;
  width: 0.75rem;
  height: 0.75rem;
  cursor: default;
  position: absolute;
  border: 1px solid #718096;
  /* margin-top: -0.125rem; */
}

.checkbox:checked + label::before {
  display: block;
  content: "\2714";
  text-align: center;
  font-size: 0.5rem;
  color: black;
  font-weight: 700;
  /* margin-top: -0.125rem; */
}

input.checkbox[type="checkbox"] {
  display: none;
}
</style>
