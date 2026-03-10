<template>
  <div v-if="isLoading" class="d-flex justify-content-center">
    <LoadingRing />
  </div>
  <div
    v-else-if="!isLoading && ibLinks.length == 0"
    class="d-flex justify-content-center"
  >
    <NoDataBox />
  </div>
  <div v-else>
    <el-card v-for="(item, index) in ibLinks.data" :key="index" class="mb-3">
      <div class="d-flex justify-content-between align-items-center">
        <div>
          {{ item.name
          }}<i
            class="fa-solid fa-pen-to-square ms-2"
            style="cursor: pointer"
            @click="editCode(item)"
          ></i>
        </div>
        <div>
          {{ item.code }}
          <TinyCopyBox :val="item.code.toString()" class="ms-1"></TinyCopyBox>
        </div>
      </div>
      <div class="d-flex justify-content-between align-items-center">
        <div>
          {{ $t("type.accountRole." + item.serviceType) }}
        </div>
        <div>
          {{
            $t(
              "type.language." +
                (item.displaySummary?.language == undefined
                  ? getLanguage
                  : item.displaySummary?.language)
            )
          }}
        </div>
      </div>
      <div class="d-flex justify-content-between align-items-center mt-2">
        <div class="text-muted fs-7">
          {{ $t("title.autoCreateAccount") }}:
          {{
            item.displaySummary?.isAutoCreatePaymentMethod === 1
              ? $t("action.yes")
              : $t("action.no")
          }}
        </div>
      </div>
      <div class="d-flex justify-content-between align-items-center mt-1">
        <div>
          <button
            type="button"
            class="btn btn-primary btn-bordered btn-sm fs"
            @click="showDetail(item.code)"
          >
            {{ $t("title.rebateSettings") }}
          </button>
        </div>
        <div>
          <CopyReferralLink
            :code="item.code"
            :siteId="salesAccount.siteId"
            :language="
              item.displaySummary?.language == undefined
                ? getLanguage
                : item.displaySummary?.language
            "
          />
        </div>
      </div>
    </el-card>
  </div>
</template>
<script setup lang="ts">
import { inject } from "vue";
import { getLanguage } from "@/core/types/LanguageTypes";
import CopyReferralLink from "@/components/CopyReferralLink.vue";
import TinyCopyBox from "@/components/TinyCopyBox.vue";

const isLoading = inject<any>("isLoading");
const ibLinks = inject<any>("ibLinks");
const editCode = inject<any>("editCode");
const salesAccount = inject<any>("salesAccount");
const showDetail = inject<any>("showDetail");
</script>
<style scoped>
@media (max-width: 768px) {
  .fs {
    font-size: 0.65rem;
  }
  .btn-outline-primary {
    border: 1px #f2f4f7 solid !important;
  }
}
</style>
