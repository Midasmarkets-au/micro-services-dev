<template id="switch-button">
  <div class="switch-button-control">
    <div
      class="switch-button"
      :class="{ enabled: props.isEnabled }"
      :style="{ '--color': props.color }"
      @click="toggle"
    >
      <div class="button"></div>
    </div>
    <div class="switch-button-label">
      <slot></slot>
    </div>
  </div>
</template>

<script setup lang="ts">
const props = defineProps<{
  isEnabled: boolean;
  color: string;
}>();

const emits = defineEmits<{
  (e: "toggleAuthType", value: boolean): void;
}>();

const toggle = () => {
  emits("toggleAuthType", !props.isEnabled);
};
</script>

<style lang="scss" scoped>
.switch-button-control {
  display: flex;
  flex-direction: row;
  align-items: center;
}
.switch-button-control .switch-button {
  display: flex;
  align-items: center;

  height: 1.6em;
  width: calc(1.6em * 2);
  border: 2px solid var(--color);
  box-shadow: inset 0px 0px 2px 0px rgba(0, 0, 0, 0.33);
  border-radius: 1.6em;
  transition: all 0.3s ease-in-out;
  cursor: pointer;
}
.switch-button-control .switch-button .button {
  height: calc(1.6em - (2 * 2px));
  width: calc(1.6em - (2 * 2px));
  border: 2px solid var(--color);
  border-radius: calc(1.6em - (2 * 2px));
  background: black;
  transition: all 0.3s ease-in-out;
}
.switch-button-control .switch-button {
  background-color: var(--color);
  box-shadow: none;
}
.switch-button-control .switch-button.enabled .button {
  background: black;
  transform: translateX(calc(calc(1.6em - (2 * 2px)) + (2 * 2px)));
}
.switch-button-control .switch-button-label {
  margin-left: 10px;
}
</style>
