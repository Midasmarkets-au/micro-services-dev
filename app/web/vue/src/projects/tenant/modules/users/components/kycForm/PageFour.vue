<template>
  <div class="page-content">
    <hr class="w-full border-black-800" />

    <div class="my-8">
      <div class="text-xs font-semibold mb-6 tracking-widerr">
        <div>COPY OF SUPPORTING DOCUMENTATION</div>
        <div>支持性⽂件复印件</div>
      </div>
      <div class="flex flex-row">
        <div class="w-1/6 text-3xs">
          <div>Type of Document:</div>
          <div>⽂件类型:</div>
        </div>
        <div class="w-5/6">
          <div class="flex flex-row">
            <div class="checkbox-wrapper">
              <input
                id="passport"
                class="checkbox"
                type="checkbox"
                name="document-type"
                disabled
              />
              <label for="passport"></label>
            </div>
            <div class="ml-3">
              <div class="field">
                <div>Passport (Country of Origin)/护照（原籍国）</div>
              </div>
            </div>
          </div>
          <div class="flex flex-row">
            <div class="checkbox-wrapper">
              <input
                id="photo-id"
                class="checkbox"
                type="checkbox"
                name="document-type"
                disabled
              />
              <label for="photo-id"></label>
            </div>
            <div class="ml-3">
              <div class="field">
                <div>
                  Photo ID / DL (Country of
                  Origin)/含照片身份证件/驾照（原籍国）
                </div>
              </div>
            </div>
          </div>
          <div class="flex flex-row">
            <div class="checkbox-wrapper">
              <input
                id="financial-statement"
                class="checkbox"
                type="checkbox"
                name="traditional-verification-1"
                disabled
              />
              <label for="financial-statement"></label>
            </div>
            <div class="ml-3">
              <div class="field">
                <div>
                  Financial Statement/Utility Bill/Government Document/
                  财务证明/水电费账单/政府文件
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <hr class="w-full border-black-800" />

    <div class="my-8">
      <div class="text-xs font-semibold mb-6 tracking-widerr">
        <div>
          You must verify the CLIENT’S FULL NAME and either their DATE OF BIRTH
          or RESIDENTIAL ADDRESS using:
        </div>
        <div>您必须利用以下资料确认客户姓名和出生日期或住址信息:</div>
      </div>
      <div class="border border-black-800 p-4">
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

    <div class="text-xs font-semibold text-center w-full tracking-widerr">
      OR / 或
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
