<template>
  <div class="card mb-5">
    <div class="card-body d-flex align-items-center justify-content-between">
      <div
        class="text-[#212121] fs-3 leading-[28px] fw-bold d-flex flex-column"
      >
        <span>积分商城活动</span>
        <span v-if="step === 1" class="credit-color d-flex flex-row gap-2">
          <span>积分</span>
          <span>{{ user.value?.credit || "1,000" }}</span>
        </span>
      </div>
      <button
        class="btn btn-sm btn-primary d-flex align-items-center gap-2"
        v-if="step === 0"
        @click="openEventRegistration()"
      >
        参加活动
      </button>
      <router-link
        :to="'/eventshop'"
        class="btn btn-sm btn-primary d-flex align-items-center gap-2"
        v-if="step === 1"
      >
        逛商城
      </router-link>
    </div>
  </div>
  <EventRegistration ref="eventRegistrationRef" />
</template>
<script lang="ts" setup>
import { useStore } from "@/store";
import { ref, computed, onMounted } from "vue";
import EventRegistration from "@/projects/client/modules/eventshop/components/modal/EventRegistration.vue";

const store = useStore();
const user = computed<any>(() => store.state.AuthModule.user);
const eventRegistrationRef = ref<any>(null);
const openEventRegistration = () => {
  const content = {
    id: 11505,
    language: "zh-cn",
    title: "Terms &Policy",
    subtitle: "",
    updatedOn: "2024-01-30T05:34:56.591453Z",
    author: "",
    content:
      '<p>有关以下产品于2024年二月份的到期日如下︰</p><p><strong>美国原油差价合约</strong>︰</p><p><strong># CLH24</strong>将于2024年2月19日 GMT 21:00到期</p><p><br></p><p><strong>布兰特原油合约</strong>︰</p><p><strong># BRNJ24</strong>将于2024年2月28日 GMT 21:00到期<br><br></p><p><em>*</em><em>如果上述中文译本内容与英文原始版本之间存在差异，则以英文版本为准。</em></p><br data-cke-eol="1"><p>有关以下产品于2024年二月份的到期日如下︰</p><p><strong>美国原油差价合约</strong>︰</p><p><strong># CLH24</strong>将于2024年2月19日 GMT 21:00到期</p><p><br></p><p><strong>布兰特原油合约</strong>︰</p><p><strong># BRNJ24</strong>将于2024年2月28日 GMT 21:00到期<br><br></p><p><em>*</em><em>如果上述中文译本内容与英文原始版本之间存在差异，则以英文版本为准。</em></p><br data-cke-eol="1"><p>有关以下产品于2024年二月份的到期日如下︰</p><p><strong>美国原油差价合约</strong>︰</p><p><strong># CLH24</strong>将于2024年2月19日 GMT 21:00到期</p><p><br></p><p><strong>布兰特原油合约</strong>︰</p><p><strong># BRNJ24</strong>将于2024年2月28日 GMT 21:00到期<br><br></p><p><em>*</em><em>如果上述中文译本内容与英文原始版本之间存在差异，则以英文版本为准。</em></p><br data-cke-eol="1">',
    topicId: 11306,
    contentInText:
      "有关以下产品于2024年二月份的到期日如下︰美国原油差价合约︰# CLH24将于2024年2月19日 GMT 21:00到期布兰特原油合约︰# BRNJ24将于2024年2月28日 GMT 21:00到期*如果上述中文译本内容与英文原始版本之间存在差异，则以英文版本为准。",
    createdOn: "2024-01-30T05:14:03.195682Z",
    effectiveFrom: "2024-01-31T16:00:00Z",
    image: "/images/bg/bcr-points-mall-banner.jpeg",
  };
  eventRegistrationRef.value?.show(content);
};
const step = ref(1);
// if the user has been registered into the event shop, do not show.
onMounted(() => {
  if (step.value === 0) {
    openEventRegistration();
  }
});
</script>

<style>
.title-color {
  color: #212121;
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
