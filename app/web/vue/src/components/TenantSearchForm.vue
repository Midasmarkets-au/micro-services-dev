<template>
  <div class="mt-2">
    <div class="input-group">
      <select class="form-select" id="searchTypeSelect" v-model="searchType">
        <option
          v-for="(option, index) in searchTypes"
          :key="index"
          :value="option.value"
        >
          {{ option.text }}
        </option>
      </select>
      <input
        type="text"
        class="form-control"
        name="searchText"
        v-model="searchText"
      />
      <button
        class="btn btn-success"
        type="button"
        @click="search"
        :disabled="isSearching"
      >
        Search
      </button>
      <span
        class="cursor-pointer mt-4 ms-4"
        @click="clear"
        v-if="searchText != ''"
      >
        Clear
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { defineProps, ref } from "vue";

const isSearching = ref(true);

const props = defineProps({
  types: {
    type: Array,
    required: true,
  },
  type: {
    type: String,
    required: true,
  },
  text: {
    type: String,
    required: true,
  },
});

const emits = defineEmits<{
  (e: "search"): void;
}>();

const searchType = ref(props.type);
const searchText = ref(props.text);
const searchTypes = ref(props.types);
const clear = () => {
  if (isSearching.value) return;
  searchText.value = "";
  search();
};

const search = () => {
  emits("search", {
    type: searchType.value,
    text: searchText.value,
  });
};

const updateSearchForm = (_type, _text) => {
  searchType.value = _type;
  searchText.value = _text;
};

const searching = (status) => {
  isSearching.value = status;
};

defineExpose({
  updateSearchForm,
  searching,
});
</script>
