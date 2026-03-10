<template>
  <div
    class="method-card-wrapper"
    :class="{
      ['selected-method-card-active']: props.selectedCategory == props.item,
    }"
    style="min-height: 160px"
  >
    <div class="d-flex justify-content-between">
      <div class="method-content">0% {{ $t("title.fee") }}</div>
      <div class="method-content">&lt; 1 {{ $t("title.hour") }}</div>
    </div>
    <hr style="margin-top: 3px; margin-bottom: 5px" />
    <div
      v-if="props.item != 'AliPay' || props.item != 'WechatPay'"
      class="method-content"
      style="text-align: right"
    >
      24/5
    </div>
    <div class="d-flex justify-content-center">
      <img
        :src="'/images/wallet/' + props.item + '.png'"
        :alt="props.item"
        style="height: 45px; max-width: 110%"
      />
    </div>
    <div class="text-center method-content" style="margin-top: 8px">
      <!-- {{ $t("type.paymentTypes." + props.item.platform) }} -->
      {{ props.item }}
    </div>
  </div>
</template>

<script setup lang="ts">
const props = defineProps<{
  item: any;
  selectedCategory?: any;
}>();
</script>

<style scoped lang="scss">
.method-card-wrapper {
  width: 200px;
  height: 120px;
  border: 1px solid #f2f4f7;
  background-color: #fafbfd;
  border-radius: 8px;
  padding: 20px;

  .method-content {
    color: #717171;
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
