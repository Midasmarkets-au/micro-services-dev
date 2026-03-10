<template>
  <div class="page-content">
    <div class="mb-4">
      <div class="text-xs flex flex-row items-center">
        <div class="mr-2 font-semibold tracking-widerr">
          Verification Method/核实方法:
        </div>
        <div class="font-normal flex flex-row">
          <div class="checkbox-wrapper" style="margin-top: 0">
            <input
              id="traditional-verification"
              class="checkbox"
              type="checkbox"
              name="verification"
              :checked="
                Number.isInteger(value.verification) && value.verification == 0
              "
              disabled
            />
            <label for="traditional-verification"></label>
          </div>
          <div class="text-xs pl-1 pr-2 ml-3">
            Traditional Verification/传统核实
          </div>
        </div>
        <div class="font-normal flex flex-row mr-2">
          <div class="checkbox-wrapper" style="margin-top: 0">
            <input
              id="electronic-verification"
              class="checkbox"
              type="checkbox"
              name="verification"
              :checked="
                Number.isInteger(value.verification) && value.verification == 1
              "
              disabled
            />
            <label for="electronic-verification"></label>
          </div>
          <div class="text-xs pl-1 ml-3">Electronic Verification/ 电子核实</div>
        </div>
        <div class="font-normal flex flex-row">
          <div class="mr-1">(</div>
          <div class="checkbox-wrapper" style="margin-top: 0">
            <input
              id="lab-group"
              class="checkbox"
              type="checkbox"
              name="e-verify-type"
              disabled
            />
            <label for="lab-group"></label>
          </div>
          <div class="text-xs pl-1 ml-3">LAB group</div>
        </div>
        <div class="font-normal flex flex-row">
          <div class="mx-1">/</div>
          <div class="checkbox-wrapper" style="margin-top: 0">
            <input
              id="data-zoo"
              class="checkbox"
              type="checkbox"
              name="e-verify-type"
              disabled
            />
            <label for="data-zoo"></label>
          </div>
          <div class="text-xs pl-1 ml-3">Data Zoo</div>
          <div class="ml-1">)</div>
        </div>
      </div>
    </div>

    <div class="mb-4 flex flex-row items-stretch justify-between">
      <div class="flex-1 pr-2 self-stretch">
        <div class="border border-black-800 w-full h-full">
          <div
            class="w-full text-xs py-1 font-semibold text-center border-b border-black-800 tracking-widerr"
          >
            Traditional Verification/传统核实
          </div>
          <div class="p-3 py-4">
            <div class="mb-3">
              <div class="text-2xs font-semibold">
                Verification method 1/核实方法1
              </div>
              <div class="pl-1 flex flex-row">
                <div class="checkbox-wrapper">
                  <input
                    id="traditional-verification-1"
                    class="checkbox"
                    type="checkbox"
                    name="traditional-verification-1"
                    disabled
                  />
                  <label for="traditional-verification-1"></label>
                </div>
                <div class="ml-3">
                  <div class="field">
                    <div>
                      Original or certified copy of a primary photographic
                      identification document/
                    </div>
                    <div>首要的含照片身份证明文件的原始件或经认证的复印/</div>
                  </div>
                </div>
              </div>
            </div>
            <div>
              <div class="text-2xs font-semibold">
                Verification method 2/核实方法2
              </div>
              <div class="pl-1 flex flex-row">
                <div class="checkbox-wrapper">
                  <input
                    id="traditional-verification-2"
                    class="checkbox"
                    type="checkbox"
                    name="traditional-verification-2"
                    disabled
                  />
                  <label for="traditional-verification-2"></label>
                </div>
                <div class="ml-3">
                  <div class="field">
                    <div>
                      Original or certified copy of a primary non-photographic
                      identification document; AND/OR
                    </div>
                    <div>
                      首要的不含照片的身份证明文件原始件或经认证的复印件;
                      以及/或
                    </div>
                    <div>
                      Original or certified copy of a secondary identification
                      document;
                    </div>
                    <div>二级身份证明文件的原始件或经认证的复印;</div>
                  </div>
                </div>
              </div>
              <!-- <div class="pl-1 flex flex-row">
                                <div class="checkbox-wrapper">
                                    <input 
                                        id="traditional-verification-2-secondary" 
                                        class="checkbox" 
                                        type="checkbox" 
                                        name="traditional-verification-2" 
                                        disabled
                                    />
                                    <label for="traditional-verification-2-secondary"></label>
                                </div>
                                <div class="ml-3">
                                    <div class="field">
                                        <div>
                                            Original or certified copy of a secondary identification document;
                                        </div>
                                        <div>
                                            二级身份证明文件的原始件或经认证的复印;
                                        </div>
                                    </div>
                                </div>
                            </div> -->
            </div>
          </div>
        </div>
      </div>

      <div class="flex-1 pl-2 self-stretch">
        <div class="border border-black-800 w-full h-full">
          <div
            class="w-full text-xs py-1 font-semibold text-center border-b border-black-800 tracking-widerr"
          >
            Electronic Verification/电子核实
          </div>
          <div class="p-3">
            <div>
              <div class="text-2xs font-semibold mb-3">
                <div>Verify information collected about a client from:</div>
                <div>收集的关于核实客户信息来源于:</div>
              </div>
              <div class="pl-1 flex flex-row">
                <div class="checkbox-wrapper">
                  <input
                    id="independent-doc"
                    class="checkbox"
                    type="checkbox"
                    name="electronic-verification"
                    disabled
                  />
                  <label for="independent-doc"></label>
                </div>
                <div class="ml-3">
                  <div class="field">
                    <div>Reliable and independent documentation</div>
                    <div>可靠且独立的文件</div>
                  </div>
                </div>
              </div>
              <div class="pl-1 flex flex-row">
                <div class="checkbox-wrapper">
                  <input
                    id="independent-e-data"
                    class="checkbox"
                    type="checkbox"
                    name="electronic-verification"
                    disabled
                  />
                  <label for="independent-e-data"></label>
                </div>
                <div class="ml-3">
                  <div class="field">
                    <div>
                      Reliable and independent electronic data from at least one
                      (1) separate data source
                    </div>
                    <div>来自至少一 (1) 个单独数据源的可靠且独立的电子</div>
                  </div>
                </div>
              </div>
              <div class="pl-1 flex flex-row">
                <div class="checkbox-wrapper">
                  <input
                    id="both-e-doc"
                    class="checkbox"
                    type="checkbox"
                    name="electronic-verification"
                    disabled
                  />
                  <label for="both-e-doc"></label>
                </div>
                <div class="ml-3">
                  <div class="field">
                    <div>A combination of both of the above</div>
                    <div>上面两者结合</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="border border-black-800 p-2 double-half-row-height mb-4">
      <div class="text-2xs font-semibold text-black-800">
        Compliance Notes/合规部门备注:
      </div>
      <div class="text-2xs text-black"></div>
    </div>

    <div class="mb-12 px-3">
      <div class="text-xs font-semibold mb-4">
        <div>
          Where a client cannot provide satisfactory evidence of identity:
        </div>
        <div>当客户无法提供令人满意的身份证明时：</div>
      </div>
      <div class="text-2xs mb-4">
        <div>
          Where a client does not possess and is unable to obtain the necessary
          information or evidence of identity, then in limited and exceptional
          cases, we may use alternative identity proofing process, which are
          commensurate with our risk-based systems and controls.
        </div>
      </div>
      <div class="text-2xs mb-4">
        <div>
          In these limited and exceptional cases, the matter will be referred to
          the AML/CTF compliance officer who may accept a self-attestation from
          the client, if they are not aware that the self-attestation is
          incorrect or misleading. The AML/CTF compliance officer will also
          designate the client as being high risk and will implement the
          appropriate level of ongoing customer due diligence procedures which
          is commensurate with a high risk client.
        </div>
      </div>
      <div class="text-2xs mb-4">
        <div>
          如果客户不拥有且无法获得必要的身份信息或证明，那么在有限和例外的情况下，我们可能会使用替代性的身份证明流程，该流程与基于风险的系统和控制措施相符。
        </div>
      </div>
      <div class="text-2xs mb-4">
        <div>
          在这些有限和例外的情况下，此事将转给可能接受客户自我证明的AML /
          CTF合规官員, 如果他们不知道该自我证明是不正确或具有误导性。AML /
          CTF合规官还将指定客户为高风险客户，并将实施与高风险客户相对应的适当级别的持续客户尽职调查程序。
        </div>
      </div>
    </div>

    <div class="flex flex-row items-center justify-between">
      <div class="flex-1">
        <hr class="w-full border-black-800 border-t-2" />
      </div>
      <div class="flex-1 text-xs font-semibold text-center tracking-widerr">
        For Office Use Only/本公司专用（请勿填写）
      </div>
      <div class="flex-1">
        <hr class="w-full border-black-800 border-t-2" />
      </div>
    </div>

    <div class="flex flex-row items-stretch justify-between mt-8">
      <div class="flex-1 pr-2 self-stretch">
        <div class="flex flex-row base-row-height border border-black-800">
          <div class="field w-1/3 border-r border-black-800">
            <div>Admin Staff Member</div>
            <div>办公室员工</div>
          </div>
          <div class="field-data w-2/3">
            {{ value.manager_name }}
          </div>
        </div>
        <div
          class="flex flex-row base-row-height border border-black-800 border-t-0"
        >
          <div class="field w-1/3 border-r border-black-800">
            <div>Position</div>
            <div>职位</div>
          </div>
          <div class="field-data w-2/3">
            {{ value.manager_position }}
          </div>
        </div>
        <div
          class="flex flex-row base-row-height border border-black-800 border-t-0"
        >
          <div class="field w-1/3 border-r border-black-800">
            <div>Signature</div>
            <div>签名</div>
          </div>
          <div class="field-data w-2/3">
            <img
              v-if="dataSignature"
              :src="dataSignature"
              alt
              style="width: 90px"
            />
          </div>
        </div>
        <div
          class="flex flex-row base-row-height border border-black-800 border-t-0"
        >
          <div class="field w-1/3 border-r border-black-800">
            <div>Date Signed</div>
            <div>签字日期</div>
          </div>
          <div class="field-data w-2/3">
            {{ value.date_signed }}
          </div>
        </div>
      </div>
      <div class="flex-1 pl-2 self-stretch">
        <div class="flex flex-row base-row-height border border-black-800">
          <div class="field w-1/3 border-r border-black-800">
            <div>Compliance Staff Member</div>
            <div>合规部门员工</div>
          </div>
          <div class="field-data w-2/3"></div>
        </div>
        <div
          class="flex flex-row base-row-height border border-black-800 border-t-0"
        >
          <div class="field w-1/3 border-r border-black-800">
            <div>Position</div>
            <div>职位</div>
          </div>
          <div class="field-data w-2/3"></div>
        </div>
        <div
          class="flex flex-row base-row-height border border-black-800 border-t-0"
        >
          <div class="field w-1/3 border-r border-black-800">
            <div>Signature</div>
            <div>签名</div>
          </div>
          <div class="field-data w-2/3"></div>
        </div>
        <div
          class="flex flex-row base-row-height border border-black-800 border-t-0"
        >
          <div class="field w-1/3 border-r border-black-800">
            <div>Date Signed</div>
            <div>签字日期</div>
          </div>
          <div class="field-data w-2/3"></div>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
export default {
  props: ["value", "signature"],
  data() {
    return {
      dataSignature: this.signature,
    };
  },
  watch: {
    signature(val) {
      this.dataSignature = val;
    },
  },
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
