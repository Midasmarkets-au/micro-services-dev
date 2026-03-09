<template>
  <div id="print" class="document-container">
    <div class="document-content">
      <div class="text-end">
        {{ getYear(verificationDetails.updatedOn) }}年
        {{ getMonth(verificationDetails.updatedOn) }}月
        {{ getDay(verificationDetails.updatedOn) }}日
      </div>
      <div>
        {{ verificationDetails.info.firstName }}
        {{ verificationDetails.info.lastName }} 様
      </div>
      <div class="d-flex justify-content-center w-full">
        <div class="title">インターネット取引口座開設認証番号のお知らせ</div>
      </div>
      <div class="content">
        <p>平素は弊社に格別のご高配を賜り、厚く御礼申し上げます。</p>
        <p>
          下記、インターネット取引口座開設の認証に使用する認証番号をお知らせいたします。
        </p>
        <p>ご確認頂けますようお願い申し上げます。</p>

        <p class="fs-4">認証番号：{{ code }}</p>

        <p>
          インターネット口座開設の画面にアクセスし、認証番号をご入力ください。
        </p>
        <p>今後とも末永くご愛顧を賜りますようお願い申し上げます。</p>
        <p class="text-end">敬具</p>
      </div>

      <div class="company-info">
        <p>あい証券株式会社</p>
        <p>東京都港区六本木1−6−1 泉ガーデンタワー7F</p>
        <p>第一種・第二種金融商品取引業（関東財務局長（金商）第236号）</p>
        <p>商品先物取引業（店頭商品デリバティブ取引）</p>
        <p>（経済産業省 20221207 商第４号、農林水産省指令４新食第2087号）</p>
        <p class="associations">
          <span>加入協会：日本証券業協会<br /></span>
          <span class="asso-ml">一般社団法人金融先物取引業協会<br /></span>
          <span class="asso-ml">日本商品先物取引協会</span>
        </p>
        <p class="contact-info">
          <span>03-3568-5088（代表）<br /></span>
          <span>03-3568-5099（FAX）<br /></span>
          <span>info@isec.jp（Eメールアドレス）</span>
        </p>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted, inject } from "vue";
import UserService from "@/projects/tenant/modules/users/services/UserService";
const isLoading = ref(false);
const isGenerated = ref(true);
const code = inject<any>("code");
const verificationDetails = inject<any>("verificationDetails");

const getVerificationCode = async () => {
  isLoading.value = true;
  try {
    const response = await UserService.getMailCode(
      verificationDetails.value.id
    );
    code.value = response;
    isGenerated.value = true;
  } catch (error) {
    console.error(error);
    isGenerated.value = false;
  } finally {
    isLoading.value = false;
  }
};

function getYear(isoString) {
  return new Date(isoString).getFullYear(); // Extracts the year
}

function getMonth(isoString) {
  return new Date(isoString).getMonth() + 1; // Extracts the month
}

function getDay(isoString) {
  return new Date(isoString).getDate(); // Extracts the day
}

onMounted(async () => {
  await getVerificationCode();
});
</script>
<style scoped>
.document-container {
  position: relative;
  width: 100%;
  max-width: 700px;
  min-height: 800px;
  margin: 40px auto;
  padding: 40px;
  border: 4px double #000; /* Creates the double line border */
}

/* Corner decorations */
/* .document-container::before,
.document-container::after,
.document-container > *::before,
.document-container > *::after {
  content: "";
  position: absolute;
  width: 20px;
  height: 20px;
  border-style: solid;
  border-color: #000;
} */

/* Top-left corner */
/* .document-container::before {
  top: -4px;
  left: -4px;
  border-width: 0 2px 2px 0;
} */

/* Top-right corner */
/* .document-container::after {
  top: -4px;
  right: -4px;
  border-width: 0px 0px 2px 2px;
} */

/* Bottom-left corner */
/* .document-container > *::before {
  bottom: -4px;
  left: -4px;
  border-width: 2px 2px 0 0;
} */

/* Bottom-right corner */
/* .document-container > *::after {
  bottom: -4px;
  right: -4px;
  border-width: 2px 0 0 2px;
} */

/* Content styling */
.document-content {
  position: relative;
  min-height: 100%;
  padding: 20px;
}

/* Date styling */
.date {
  text-align: right;
  margin-bottom: 30px;
}

/* Title styling */
.title {
  text-align: center;
  margin: 20px 0;
  font-size: 16px;
  font-weight: bold;
  width: max-content;
  background-color: #e5e5e5;
  padding: 5px;
}

/* Company info styling */
.company-info {
  margin-top: 40px;
  margin-left: 100px;
}

.company-info p {
  margin: 5px 0;
  line-height: 1.6;
}

.asso-ml {
  margin-left: 67px;
}
.contact-info {
  margin-left: 93px !important;
}
</style>
