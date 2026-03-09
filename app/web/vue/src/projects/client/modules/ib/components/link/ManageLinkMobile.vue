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
    <el-card v-for="(item, index) in ibLinks" :key="index" class="mb-3">
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
          {{ $t("type.accountRole." + item.serviceType) }}
        </div>
      </div>
      <div class="d-flex justify-content-between align-items-center my-2">
        <div>
          {{ item.code }}
          <TinyCopyBox :val="item.code.toString()" class="ms-1"></TinyCopyBox>
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
      <div class="d-flex justify-content-between align-items-center my-2">
        <div class="text-muted fs-7">
          {{ $t("title.autoCreateAccount") }}:
          {{
            item.summary?.isAutoCreatePaymentMethod === 1
              ? $t("action.yes")
              : $t("action.no")
          }}
        </div>
      </div>
      <div class="d-flex justify-content-between align-items-center mt-1">
        <div>
          <button
            type="button"
            class="btn btn-primary fs"
            @click="showDetail(item.code)"
          >
            {{ $t("title.rebateSettings") }}
          </button>
        </div>
        <div>
          <CopyReferralLink
            :code="item.code"
            :siteId="siteId"
            :language="
              item.summary.language == undefined
                ? getLanguage
                : item.summary.language
            "
            @click="confirmCopy(item)"
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
const showDetail = inject<any>("showDetail");
const siteId = inject<any>("siteId");
const confirmCopy = inject<any>("confirmCopy");
</script>
<style scoped>
@media (max-width: 768px) {
  .fs {
    font-size: 0.65rem;
  }
  .btn-outline-primary {
    border: #000 solid !important;
  }
}
</style>
