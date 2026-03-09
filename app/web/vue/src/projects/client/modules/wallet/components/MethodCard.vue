<template>
  <div
    class="method-card-wrapper"
    :class="{
      ['selected-method-card-active']:
        selectedId == props.item.id || selectedId == 0,
    }"
    style="min-height: 160px"
  >
    <div class="d-flex justify-content-between">
      <div class="method-content">0% {{ $t("title.fee") }}</div>
      <div class="method-content">&lt; 1 {{ $t("title.hour") }}</div>
    </div>
    <hr style="margin-top: 3px; margin-bottom: 5px" />
    <div class="method-content" style="text-align: right">24/5</div>
    <div class="d-flex justify-content-center">
      <img
        :src="props.item.logo"
        :alt="props.item.description"
        style="height: 45px; max-width: 110%"
      />
    </div>
    <div class="text-center method-content" style="margin-top: 8px">
      {{ props.item.name }}
    </div>
  </div>
</template>

<script setup lang="ts">
const props = defineProps<{
  item: any;
  selectedId?: number;
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
    height: 170px;
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
