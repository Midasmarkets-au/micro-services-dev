<template>
  <SiderDetail
    :save="submit"
    :discard="close"
    :title="$t('action.edit')"
    elId="rebate_detail_show"
    :isLoading="isLoading"
    :submited="submited"
    :isDisabled="false"
    :savingTitle="$t('action.saving')"
    :show-footer="false"
    width="{default:'40%'}"
    ref="rebateDetailShowRef"
  >
    <div class="row">
      <div class="col-4 mb-5" v-for="(item, key) in rebates[0]" :key="key">
        <label class="fs-5 fw-semobold mb-2 required">{{ key }}</label>
        <input
          class="form-control form-control-solid"
          placeholder=""
          name="rate"
          :value="item"
          disabled
        />
      </div>
    </div>
  </SiderDetail>
</template>

<script setup lang="ts">
import { ref } from "vue";
import RebateService from "../services/RebateService";
import SiderDetail from "@/components/SiderDetail.vue";

const isLoading = ref(true);
const submited = ref(false);
const rebates = ref({} as any);
const rebateDetailShowRef = ref<InstanceType<typeof SiderDetail>>();

const criteria = ref({
  Id: 0,
});

const show = async (_id: number) => {
  rebateDetailShowRef.value?.show();

  try {
    criteria.value.Id = _id;
    const responseBody = await RebateService.getTradeFromRebate(criteria.value);
    rebates.value = responseBody.data;
    criteria.value = responseBody.criteria;

    isLoading.value = false;
  } catch (error) {
    console.log(error);
  }
};

const submit = async () => {
  isLoading.value = true;
};

const close = () => {
  rebateDetailShowRef.value?.hide();
};

defineExpose({ show });
</script>
