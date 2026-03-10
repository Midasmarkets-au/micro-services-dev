import { Modal } from "bootstrap";

const hideModal = (modalEl: HTMLElement | null): void => {
  if (!modalEl) {
    return;
  }
  const myModal = Modal.getInstance(modalEl);
  myModal.hide();
};

const showModal = (modalEl: HTMLElement | null): void => {
  if (!modalEl) {
    return;
  }
  const myModal = Modal.getOrCreateInstance(modalEl);
  myModal.show();
  requestAnimationFrame(() => {
    document.body.style.overflow = ""; // Remove overflow:hidden from body
  });
};

const removeModalBackdrop = (): void => {
  if (document.querySelectorAll(".modal-backdrop.fade.show").length) {
    document.querySelectorAll(".modal-backdrop.fade.show").forEach((item) => {
      item.remove();
    });
  }
};

export { removeModalBackdrop, hideModal, showModal };
