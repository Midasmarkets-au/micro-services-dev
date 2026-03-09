<template>
  <div
    class="card ratio overflow-hidden"
    style="--bs-aspect-ratio: 25%; border: 0 !important"
  >
    <div
      class="card-body d-flex align-items-start justify-content-center flex-column banner-bg gap-3"
    >
      <div class="position-absolute bottom-5 end-2">{{ eventDetail }}</div>
      <div
        class="text-[#212121] fs-3 leading-[28px] fw-bold d-flex flex-column gap-1"
      >
        <h3 class="fw-bold banner-title">积分商城活动</h3>
        <div class="d-flex flex-row gap-5 items-content-center">
          <span class="d-flex flex-row gap-5">
            <span class="banner-detail">开始日期</span>
            <span class="banner-detail_value">01/01/24</span>
          </span>
          <span class="d-flex flex-row gap-5">
            <span class="banner-detail">活动周期</span>
            <span class="banner-detail_value">长期</span>
          </span>
        </div>
      </div>
      <div class="d-flex flex-row items-content-center gap-5">
        <!-- click open modal -->
        <button
          class="btn btn-sm btn-primary d-flex align-items-center gap-2"
          v-if="!$cans(['eventShop'])"
          @click="openEventDescription()"
        >
          了解更多
        </button>
        <router-link
          :to="'/eventshop'"
          class="btn-sm btn d-flex align-items-center gap-2 outline-button"
          v-if="!$cans(['eventShop'])"
        >
          积分商城
        </router-link>
      </div>
    </div>
  </div>
  <EventDetailsCard ref="eventDescriptionRef" />
</template>
<script lang="ts" setup>
import { useStore } from "@/store";
import { ref, computed } from "vue";
import EventDetailsCard from "../modules/supports/components/EventDetailsCard.vue";

const store = useStore();
const user = computed<any>(() => store.state.AuthModule.user);
const eventDetail = ref<string>(`*活动细则请咨询官方客服`);
const eventDescriptionRef = ref<any>(null);
const openEventDescription = () => {
  const content = {
    id: 11505,
    language: "zh-cn",
    title: "积分商城活动细则",
    subtitle: "",
    updatedOn: "2024-01-30T05:34:56.591453Z",
    author: "",
    content:
      '<p>有关以下产品于2024年二月份的到期日如下︰</p><p><strong>美国原油差价合约</strong>︰</p><p><strong># CLH24</strong>将于2024年2月19日 GMT 21:00到期</p><p><br></p><p><strong>布兰特原油合约</strong>︰</p><p><strong># BRNJ24</strong>将于2024年2月28日 GMT 21:00到期<br><br></p><p><em>*</em><em>如果上述中文译本内容与英文原始版本之间存在差异，则以英文版本为准。</em></p><br data-cke-eol="1">',
    topicId: 11306,
    contentInText:
      "有关以下产品于2024年二月份的到期日如下︰美国原油差价合约︰# CLH24将于2024年2月19日 GMT 21:00到期布兰特原油合约︰# BRNJ24将于2024年2月28日 GMT 21:00到期*如果上述中文译本内容与英文原始版本之间存在差异，则以英文版本为准。",
    createdOn: "2024-01-30T05:14:03.195682Z",
    effectiveFrom: "2024-01-31T16:00:00Z",
    image: "/images/bg/bcr-points-mall-banner.jpeg",
  };
  eventDescriptionRef.value?.show(content);
};
</script>

<style>
.banner-bg {
  background-image: url("/images/bg/event-shop.png");
  background-size: cover;
  background-repeat: no-repeat;
  min-height: 22rem;
}
.banner-title {
  font-style: normal;
  font-weight: 400;
  font-size: 28px;
  line-height: 36px;
  color: #212121;
}
.banner-detail {
  font-style: normal;
  font-weight: 400;
  font-size: 14px;
  line-height: 20px;
  color: #fff;
}
.banner-detail_value {
  font-style: normal;
  font-weight: 400;
  font-size: 14px;
  line-height: 20px;
  color: #fff;
}
.outline-button {
  border: 1px solid #212121 !important;
  color: #212121;
  box-sizing: border-box;
}
.outline-button:hover {
  background-color: #ffce00 !important;
  border: 1px solid #ffce00 !important;
}
.credit-color {
  font-style: normal;
  font-weight: 400;
  font-size: 14px;
  line-height: 20px;
  color: #f7b93f;
}
.role-badge {
  padding: 2px 16px;
  font-weight: 500;
  border-radius: 8px;
}
</style>
