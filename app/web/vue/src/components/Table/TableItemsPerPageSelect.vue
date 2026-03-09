<template>
  <div
    class="d-flex align-items-center justify-content-center justify-content-md-start"
    :class="{
      'col-sm-12 col-md-5': itemsPerPageDropdownEnabled,
    }"
  >
    <!-- <label for="items-per-page">
      <select
        class="form-select form-select-sm form-select-solid"
        v-if="itemsPerPageDropdownEnabled"
        v-model="itemsCountInTable"
        name="items-per-page"
        id="items-per-page"
      >
        <option v-for="pageSize in pageSizes" :key="pageSize" :value="pageSize">
          {{ pageSize }}
        </option>
      </select>
    </label> -->
  </div>
</template>

<script lang="ts">
import {
  defineComponent,
  ref,
  onMounted,
  WritableComputedRef,
  computed,
} from "vue";

export default defineComponent({
  name: "table-items-per-page-select",
  components: {},
  props: {
    itemsPerPage: { type: Number, default: 10 },
    itemsPerPageDropdownEnabled: {
      type: Boolean,
      required: false,
      default: true,
    },
  },
  emits: ["update:itemsPerPage"],
  setup(props, { emit }) {
    const inputItemsPerPage = ref(10);
    const pageSizes = [10, 15, 20, 25, 30, 50, 100];

    onMounted(() => {
      inputItemsPerPage.value = props.itemsPerPage;
    });

    const itemsCountInTable: WritableComputedRef<number> = computed({
      get(): number {
        return props.itemsPerPage;
      },
      set(value: number): void {
        console.log("update:itemsPerPage", value);
        emit("update:itemsPerPage", value);
      },
    });

    return {
      itemsCountInTable,
      pageSizes,
    };
  },
});
</script>
