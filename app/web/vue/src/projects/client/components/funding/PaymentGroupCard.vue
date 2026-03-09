<template>
  <div
    class="method-card-wrapper"
    :class="{
      ['selected-method-card-active']: props.selectedGroup == props.groupName,
    }"
    style="min-height: 160px"
  >
    <div class="d-flex justify-content-between">
      <div class="method-content">0% {{ $t("title.fee") }}</div>
      <div class="method-content">&lt; 1 {{ $t("title.hour") }}</div>
    </div>
    <hr style="margin-top: 3px; margin-bottom: 5px" />
    <div
      v-if="props.groupName != 'AliPay' && props.groupName != 'WechatPay'"
      class="method-content"
      style="text-align: right"
    >
      24/5
    </div>
    <div class="d-flex justify-content-center">
      <img
        :src="props.logo"
        :alt="props.showName"
        style="height: 45px; max-width: 110%"
      />
    </div>
    <div class="text-center method-content" style="margin-top: 8px">
      {{ props.showName }}
    </div>
    <!-- <div class="range-label" style="margin-top: 8px">
      <span>{{ $t("fields.initMinAmount") }} :</span>
      <BalanceShow :balance="item?.initialValue * 100"></BalanceShow>
    </div> -->
    <div class="range-label" tyle="margin-top: 10px">
      <span>{{ $t("fields.minAmount") }} :</span>
      <BalanceShow :balance="item?.range[0] * 100"></BalanceShow>
    </div>
    <div class="range-label">
      <span>{{ $t("fields.maxAmount") }} :</span>
      <BalanceShow :balance="item?.range[1] * 100"></BalanceShow>
    </div>
  </div>
</template>

<script setup lang="ts">
const props = defineProps<{
  item: any;
  logo?: string;
  groupName: string;
  showName: string;
  selectedGroup?: any;
}>();
</script>

<style scoped lang="scss">
.method-card-wrapper {
  width: 200px;
  border: 1px solid #f2f4f7;
  background-color: #fafbfd;
  border-radius: 8px;
  padding: 10px 20px;

  .method-content {
    color: #717171;
  }

  .range-label {
    font-size: 11px;
    color: #717171;
    display: flex;
    justify-content: space-between;
  }

  .amount-range-value {
    font-size: 14px;
    font-weight: 600;
    color: #1a1a1a;
    display: flex;
    align-items: center;
    gap: 4px;
    justify-content: center;
  }

  @media (max-width: 768px) {
    width: 145px;
    height: 120px;
  }
}

.selected-method-card-active {
  border: 1px dashed #000f32;
}

.method-card-wrapper:hover {
  border: 1px dashed #000f32;
  cursor: pointer;
}
</style>
